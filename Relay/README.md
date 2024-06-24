# OrdureX Relay

# Running the Relay

> [!Note]
> You will need to have Node.js installed. You can download it from [here](https://nodejs.org/en/download/).

```bash
npm install
npm run start
```

# Testing

To test the server you may pipe the example payloads to netcat:

```bash
nc 127.0.0.1 7070 < tests/paylods/status-simulation.bin
```
