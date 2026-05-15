# IOT_MQTT_PROTOCOL

Status: DRAFT
Last validated: 2026-05-15

## Pregled

IoT MQTT protokol definise komunikaciju izmedju hotelskog sistema i pametnih uredjaja u sobama (brave, senzori, HVAC). Detaljna specifikacija topic strukture i payload formata nalazi se u `../fsd/FSD_14_IOT_INTEGRACIJE.md`.

## Broker

- Tip: Mosquitto ili HiveMQ
- Port: 1883 (dev), 8883 (prod/TLS)
- Autentifikacija: username/password za dev, certifikati za prod

## QoS nivoi

| Tip poruke | QoS | Razlog |
|------------|-----|--------|
| Lock command | 1 (at least once) | Komanda mora stici |
| Sensor data | 0 (at most once) | Tolerantno na gubitak |
| HVAC command | 1 (at least once) | Komanda mora stici |
| Energy report | 0 (at most once) | Periodicni podaci |

## Retained poruke

- `lock/status` — RETAINED (zadnji poznati status brave)
- `sensor/temperature` — RETAINED (zadnja temperatura)
- `sensor/occupancy` — RETAINED (zadnji status prisutnosti)
- Komande (`lock/command`, `hvac/command`) — NIKADA retained

## Restrikcije

- Svaka poruka MORA sadrzavati `device_id` i `timestamp`
- IoT servis ne smije pisati direktno u bazu — koristi backend API
- Brave moraju raditi offline ako MQTT padne
- Maksimalna velicina poruke: 8KB
