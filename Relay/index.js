import net from 'net';
import { stringify as uuidStringify } from 'uuid';

const PORT = 7070;
const HOST = '127.0.0.1';

const server = net.createServer();

server.listen(PORT, HOST, () => {
    console.log(`OrdureX relay server is running on port ${PORT}`);
});

/** @type {net.Socket[]} */
let sockets = [];

server.on('connection', (sock) => {
    console.log(`New connection from ${sock.remoteAddress}:${sock.remotePort} (${sockets.length + 1} total)`);
    sockets.push(sock);

    sock.on('data', (buf) => {
        console.log(`Data received (${sock.remoteAddress}): ${buf.length} bytes`);
        try {
            decodePackedData(buf);
        } catch (e) {
            console.error('Paylaod decode error', e);
        }
    });

    sock.on('close', () => {
        sockets = sockets.filter((s) => !(s.remoteAddress === sock.remoteAddress && s.remotePort === sock.remotePort));
        console.log(`Connection closed ${sock.remoteAddress}:${sock.remotePort} (${sockets.length} total)`);
    });
});

/**
 * @param {Buffer} buf
 */
function decodePackedData(buf) {
    const statusCount = buf.readUInt8(0);
    let offset = 1;
    console.log('=====================');
    console.log(`Status count: ${statusCount}`);
    for (let s = 0; s < statusCount; s++) {
        const statusId = buf.readUInt8(offset++);

        console.log('---');
        console.log(`#${s} status ID: ${statusId}`);
        if (statusId >= 0 && statusId < 3) {
            const trashId = statusId;
            console.log(`#${s} status name: status/trash-${trashId}/collect-requested`);
        } else if (statusId >= 3 && statusId < 6) {
            const trashId = statusId - 3;
            console.log(`#${s} status name: status/trash-${trashId}/invalid-code`);
            const clientId = uuidStringify(buf.subarray(offset, offset + 16));
            offset += 16;
            console.log(`#${s} client UUID: ${clientId}`);
        } else if (statusId == 6) {
            console.log(`#${s} status name: status/trash-1/burning`);
        } else if (statusId >= 7 && statusId < 9) {
            const trashId = statusId === 7 ? 0 : 2;
            console.log(`#${s} status name: status/trash-${trashId}/lid`);
            const lidState = buf.readUInt8(offset++);
            console.log(`#${s} lid is ${lidState === 1 ? 'open' : 'closed'}`);
        } else if (statusId === 9) {
            console.log(`#${s} status name: status/simulation`);
            const status = buf.readUInt8(offset++);
            console.log(`#${s} simulation status: ${status === 0 ? 'ready/inactive' : 'busy/active'}`);
            if (status === 1) {
                const clientId = uuidStringify(buf.subarray(offset, offset + 16));
                offset += 16;
                console.log(`#${s} client UUID: ${clientId}`);
            }
        }
    }
}
