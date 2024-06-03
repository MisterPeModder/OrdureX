// @ts-check

import net from 'net';
import { stringify as uuidStringify, parse as uuidParse } from 'uuid';
import mqtt from 'mqtt';

const PORT = 7070;
const HOST = '0.0.0.0';
const MQTT_URL = 'ws://broker.hivemq.com:8000/mqtt';

const tcpServer = net.createServer();
const mqttClient = mqtt.connect(MQTT_URL);
let mqttClientReady = false;

/** @type {net.Socket[]} */
let sockets = [];
const actionsQueue = [];

mqttClient.on('error', (err) => {
    console.error('MQTT error', err);
    process.exit(1);
});

tcpServer.listen(PORT, HOST, () => {
    console.log(`OrdureX relay server is running on port ${PORT}`);
});

tcpServer.on('connection', (sock) => {
    console.log(`New connection from ${sock.remoteAddress}:${sock.remotePort} (${sockets.length + 1} total)`);
    sockets.push(sock);

    sock.on('data', (buf) => {
        try {
            decodePackedData(buf);
            flushActionQueue(sock);
        } catch (e) {
            console.error('Paylaod decode error', e);
        }
    });

    sock.on('close', () => {
        sockets = sockets.filter((s) => !(s.remoteAddress === sock.remoteAddress && s.remotePort === sock.remotePort));
        console.log(`Connection closed ${sock.remoteAddress}:${sock.remotePort} (${sockets.length} total)`);
    });
});

mqttClient.on('connect', () => {
    console.log('Connected to MQTT broker');
    mqttClient.subscribe(
        {
            'ordurex/action/trash-0/lid': {
                qos: 1,
            },
            'ordurex/action/trash-2/lid': {
                qos: 1,
            },
            'ordurex/action/trash-1/buzzer': {
                qos: 2,
            },
            'ordurex/action/trash-2/display': {
                qos: 2,
            },
            'ordurex/action/trash-0/request-collect': {
                qos: 1,
            },
            'ordurex/action/trash-1/request-collect': {
                qos: 1,
            },
            'ordurex/action/trash-2/request-collect': {
                qos: 1,
            },
            'ordurex/action/simulation': {
                qos: 2,
            },
        },
        (err) => {
            if (err) {
                console.error('MQTT subscription error', err);
                process.exit(1);
            } else {
                mqttClientReady = true;
            }
        }
    );
});

mqttClient.on('message', (topic, message) => {
    console.log('=====================');
    console.log(`Received from MQTT, topic ${topic} (${message.length} bytes)`);

    if (topic === 'ordurex/action/trash-0/lid') {
        const lidState = message.readUInt8(0);
        console.log(`> Lid state for trash-0: ${lidState === 1 ? 'open' : 'closed'}`);
        actionsQueue.push({
            type: 'lid',
            trashId: 0,
            lidState,
        });
    } else if (topic === 'ordurex/action/trash-2/lid') {
        const lidState = message.readUInt8(0);
        console.log(`> Lid state for trash-2: ${lidState === 1 ? 'open' : 'closed'}`);
        actionsQueue.push({
            type: 'lid',
            trashId: 2,
            lidState,
        });
    } else if (topic === 'ordurex/action/trash-1/buzzer') {
        const buzzerState = message.readUInt8(0);
        console.log(`> Buzzer state for trash-1: ${buzzerState}`);
        actionsQueue.push({
            type: 'buzzer',
            trashId: 1,
            buzzerState,
        });
    } else if (topic === 'ordurex/action/trash-2/display') {
        const messageStr = message.toString('utf-8');
        console.log(`> Displaying message on trash-2: ${messageStr}`);
        actionsQueue.push({
            type: 'display',
            trashId: 2,
            message: messageStr,
        });
    } else if (topic === 'ordurex/action/trash-0/request-collect') {
        const clientId = uuidStringify(message.subarray(0, 16));
        const code = message.toString('utf-8', 16);
        console.log(`> Displaying collect of trash-0: client = ${clientId}, code = ${'*'.repeat(code.length)}`);
        actionsQueue.push({
            type: 'collect',
            trashId: 0,
            clientId,
            code,
        });
    } else if (topic === 'ordurex/action/trash-1/request-collect') {
        const clientId = uuidStringify(message.subarray(0, 16));
        const code = message.toString('utf-8', 16);
        console.log(`> Displaying collect of trash-1: client = ${clientId}, code = ${'*'.repeat(code.length)}`);
        actionsQueue.push({
            type: 'collect',
            trashId: 1,
            clientId,
            code,
        });
    } else if (topic === 'ordurex/action/trash-2/request-collect') {
        const clientId = uuidStringify(message.subarray(0, 16));
        const code = message.toString('utf-8', 16);
        console.log(`> Displaying collect of trash-2: client = ${clientId}, code = ${'*'.repeat(code.length)}`);
        actionsQueue.push({
            type: 'collect',
            trashId: 2,
            clientId,
            code,
        });
    } else if (topic === 'ordurex/action/simulation') {
        const simulationState = message.readUInt8(0);
        console.log(message.subarray(1, 17));
        const clientId = uuidStringify(message.subarray(1, 17));
        console.log(
            `> ${simulationState === 0 ? 'Stopping' : simulationState === 1 ? 'Starting' : 'Pausing'} simulation, from client ${clientId}`
        );
        actionsQueue.push({
            type: 'simulation',
            simulationState,
            clientId,
        });
    }

    console.log('Action queue depth: ', actionsQueue.length);
});

/**
 * @param {Buffer} buf
 */
function decodePackedData(buf) {
    if (!mqttClientReady) {
        console.error('Cannot process incoming data, MQTT client is not ready');
        return;
    }

    const statusCount = buf.readUInt8(0);
    let offset = 1;
    console.log('=====================');
    console.log(`Received from Arduino (${buf.length} bytes), status count: ${statusCount}`);
    for (let s = 0; s < statusCount; s++) {
        const statusId = buf.readUInt8(offset++);

        console.log('---');
        console.log(`#${s} status ID: ${statusId}`);
        if (statusId >= 0 && statusId < 3) {
            const trashId = statusId;
            console.log(`#${s} status name: ordurex/status/trash-${trashId}/collect-requested`);

            mqttClient.publish(`ordurex/status/trash-${trashId}/collect-requested`, Buffer.from([]), { qos: 1 });
        } else if (statusId >= 3 && statusId < 6) {
            const trashId = statusId - 3;
            console.log(`#${s} status name: ordurex/status/trash-${trashId}/invalid-code`);
            const rawClientId = buf.subarray(offset, offset + 16);
            const clientId = uuidStringify(rawClientId);
            offset += 16;
            console.log(`#${s} client UUID: ${clientId}`);

            mqttClient.publish(`ordurex/status/trash-${trashId}/invalid-code`, rawClientId, { qos: 1 });
        } else if (statusId == 6) {
            console.log(`#${s} status name: ordurex/status/trash-1/burning`);

            mqttClient.publish('ordurex/status/trash-1/burning', Buffer.from([]), { qos: 1 });
        } else if (statusId >= 7 && statusId < 9) {
            const trashId = statusId === 7 ? 0 : 2;
            console.log(`#${s} status name: ordurex/status/trash-${trashId}/lid`);
            const lidState = buf.readUInt8(offset++);
            console.log(`#${s} lid is ${lidState === 1 ? 'open' : 'closed'}`);

            mqttClient.publish(`ordurex/status/trash-${trashId}/lid`, Buffer.from([lidState]), {
                qos: 1,
                retain: true,
            });
        } else if (statusId === 9) {
            console.log(`#${s} status name: ordurex/status/simulation`);
            const status = buf.readUInt8(offset++);
            console.log(`#${s} simulation status: ${status === 0 ? 'ready/inactive' : 'busy/active'}`);
            if (status === 1) {
                const rawClientId = buf.subarray(offset, offset + 16);
                const clientId = uuidStringify(rawClientId);
                offset += 16;
                console.log(`#${s} client UUID: ${clientId}`);

                mqttClient.publish('ordurex/status/simulation', Buffer.concat([Buffer.from([status]), rawClientId]), {
                    qos: 1,
                    retain: true,
                });
            } else {
                mqttClient.publish('ordurex/status/simulation', Buffer.from([status]), { qos: 1, retain: true });
            }
        }
    }
}

/**
 * @param {net.Socket} sock
 */
function flushActionQueue(sock) {
    console.log('=====================');
    console.log('Flushing action queue, count: ', actionsQueue.length);

    const toSendCount = Math.min(actionsQueue.length, 256);
    let headerBuf = Buffer.from([toSendCount]);
    /** @type {Buffer[]} */
    let actionBufs = [];

    for (let s = 0; s < toSendCount; s++) {
        /** @type {Buffer} */
        let actionBuf;
        let action = actionsQueue.shift();

        console.log('---');
        console.log('type: ', action.type);

        if (action.type === 'lid') {
            actionBuf = Buffer.from([action.trashId === 0 ? 0 : 2, action.lidState]);
        } else if (action.type === 'buzzer') {
            actionBuf = Buffer.from([2, action.buzzerState]);
        } else if (action.type === 'display') {
            const messageBuf = Buffer.from(action.message, 'utf-8').subarray(0, 256);
            actionBuf = Buffer.concat([Buffer.from([3, messageBuf.length]), messageBuf]);
        } else if (action.type === 'collect') {
            const clientIdBuf = uuidParse(action.clientId);
            const codeBuf = Buffer.from(action.code, 'utf-8').subarray(0, 256);
            actionBuf = Buffer.concat([
                Buffer.from([4 + action.trashId]),
                clientIdBuf,
                Buffer.from([codeBuf.length]),
                codeBuf,
            ]);
        } else if (action.type === 'simulation') {
            actionBuf = Buffer.concat([Buffer.from([7, action.simulationState]), uuidParse(action.clientId)]);
        } else {
            console.error('Unknown action type: ', action.type, action);
            break;
        }

        actionBufs.push(actionBuf);
    }

    console.log('---');
    console.log(`Sending response to Arduino: ${headerBuf.length} bytes`);
    sock.write(Buffer.concat([headerBuf, ...actionBufs]));
}
