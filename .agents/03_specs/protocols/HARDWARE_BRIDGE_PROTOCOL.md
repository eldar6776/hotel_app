# HARDWARE_BRIDGE_PROTOCOL

Status: DRAFT
Last validated: 2026-05-15

## Pregled

Hardware Bridge je lokalni servis koji se instalira na racunar koji ima fizicki prikljucene USB/Serial uredjaje (fiskalne kase, RFID citace kartica). Bridge sluzi kao posrednik izmedju web aplikacije (frontend u browseru) i fizickog hardvera.

## Arhitektura

```
[Browser/Frontend] <--HTTP/WebSocket--> [Hardware Bridge (localhost:9100)] <--USB/Serial--> [Fizicki uredjaj]
```

## Komunikacioni tok

1. Frontend salje HTTP POST na `http://localhost:9100/api/fiscal/print` sa podacima racuna
2. Bridge prima zahtjev, formatira podatke za fiskalni printer
3. Bridge salje komandu preko USB/Serial porta
4. Bridge prima odgovor od printera
5. Bridge vraca JSON odgovor frontendu

## Endpointi (planirani)

- `POST /api/fiscal/print` — stampaj fiskalni racun
- `POST /api/fiscal/daily-report` — dnevni fiskalni izvjestaj
- `POST /api/card/write` — programiraj RFID karticu za gosta
- `POST /api/card/read` — procitaj RFID karticu
- `GET /api/status` — status konektovanih uredjaja

## Cross-platform

- Bridge mora raditi na Windows i macOS
- Preporuceni stack: .NET 8 Worker Service ili Electron
- Serial komunikacija: `System.IO.Ports` (.NET) ili `serialport` (Node.js)

## Restrikcije

- Bridge NIKADA ne pristupa bazi direktno
- Bridge NIKADA ne cuva podatke lokalno
- Bridge samo proslijedjuje komande i odgovore
- TLS za komunikaciju Bridge <-> Frontend u produkciji
