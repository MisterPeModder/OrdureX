// @ts-check

'use strict';

import { unsafeStringify, uuidParse } from '../uuid.js';
import net from 'net';

const PORT = 7070;
const HOST = '127.0.0.1';
const REQUEST_INTERVAL = 1000;

const mockTcpClient = new net.Socket();
const statusQueue = [];

let connectionInProgress = false;
let connected = false;
let queuedStatus = [];
let simState = 0;
let currentClient = null;

doConnect();

function doConnect() {
    // if (connectionInProgress || connected) return;

    // connectionInProgress = true;
    console.log('Connecting to server...');
    mockTcpClient.connect(PORT, HOST, () => {
        // connectionInProgress = false;
        // connected = true;
        console.log('Mock client connected');
        setInterval(doRequest, REQUEST_INTERVAL);
    });
}

mockTcpClient.on('error', (err) => {
    if (err.code === 'ECONNREFUSED') {
        // connected = false;
        console.error('Connection failed');
        // setTimeout(doConnect, 3000);
    }
});

mockTcpClient.on('close', () => {
    // connected = false;
    // connectionInProgress = false;
    console.log('Mock client disconnected');
    process.exit(0);
    // connectionInProgress = false;
    // setTimeout(doConnect, 3000);
});

mockTcpClient.on('data', (buf) => {
    console.log('=====================');
    const actionCount = buf.readUint8(0);
    console.log(`Recieved ${actionCount} actions from server`);

    let offset = 1;

    for (let s = 0; s < actionCount; s++) {
        console.log('---');
        const actionType = buf.readUint8(offset++);

        if (actionType === 0) {
            console.log(`#${s} status name: ordurex/actions/trash-0/lid`);
        } else if (actionType === 1) {
            console.log(`#${s} status name: ordurex/actions/trash-2/lid`);
        } else if (actionType === 2) {
            console.log(`#${s} status name: ordurex/actions/trash-1/buzzer`);
        } else if (actionType === 3) {
            console.log(`#${s} status name: ordurex/actions/trash-2/display`);
        } else if (actionType === 4) {
            console.log(`#${s} status name: ordurex/actions/trash-0/request-collect`);
        } else if (actionType === 5) {
            console.log(`#${s} status name: ordurex/actions/trash-1/request-collect`);
        } else if (actionType === 6) {
            console.log(`#${s} status name: ordurex/actions/trash-2/request-collect`);
        } else if (actionType === 7) {
            console.log(`#${s} status name: ordurex/actions/simulation`);
            simState = buf.readUint8(offset++);
            const z = buf.subarray(offset, offset + 16);
            console.log('Client: ', z.length);
            currentClient = unsafeStringify(z);
            offset += 16;
            statusQueue.push({ type: 'simulation', value: simState, client: currentClient });
        } else {
        }
    }
});

function doRequest() {
    console.log('=====================');
    console.log('Flushing status queue, status count: ', statusQueue.length);

    const toSendCount = Math.min(statusQueue.length, 256);
    let headerBuf = Buffer.from([toSendCount]);
    /** @type {Buffer[]} */
    let statusBufs = [];

    for (let s = 0; s < toSendCount; s++) {
        /** @type {Buffer} */
        let statusBuf;
        let status = statusQueue.shift();

        if (status.type === 'simulation') {
            /** @type {Uint8Array} */
            let clientBuf;

            if (status.client) {
                clientBuf = uuidParse(status.client);
            } else {
                clientBuf = Buffer.alloc(16);
            }
            statusBuf = Buffer.concat([Buffer.from([9, status.value]), clientBuf]);
            console.log('Sending simulation status: ', statusBuf.length);
        } else {
            console.error('Unknown status type: ', status.type, status);
            break;
        }

        statusBufs.push(statusBuf);
    }

    mockTcpClient.write(Buffer.concat([headerBuf, ...statusBufs]));
}
