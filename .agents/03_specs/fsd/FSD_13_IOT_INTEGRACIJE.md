# FSD 13: Smart Hotel IoT Integracije

Status: AUTHORITATIVE
Last validated: 2026-05-15

## 1. Cilj

Integrisati pametne uredjaje u hotelski sistem radi poboljsanja iskustva gostiju i energetske efikasnosti. Sistem koristi MQTT protokol za komunikaciju sa IoT uredjajima.

## 2. Komponente

### 2.1 MQTT Broker

- Lokacija: `iot_services/`
- Broker: Mosquitto ili HiveMQ (Docker kontejner)
- Port: 1883 (plaintext), 8883 (TLS)

### 2.2 Topic struktura

```
hotel/{hotel_id}/room/{room_number}/lock/command     — komanda za bravu
hotel/{hotel_id}/room/{room_number}/lock/status       — status brave
hotel/{hotel_id}/room/{room_number}/sensor/temperature — temperatura
hotel/{hotel_id}/room/{room_number}/sensor/occupancy   — prisutnost gosta
hotel/{hotel_id}/room/{room_number}/sensor/window      — prozor otvoren/zatvoren
hotel/{hotel_id}/room/{room_number}/hvac/command       — komanda za klimu
hotel/{hotel_id}/room/{room_number}/hvac/status        — status klime
hotel/{hotel_id}/room/{room_number}/energy/consumption — potrosnja
```

### 2.3 Payload format (JSON)

```json
{
  "device_id": "lock_101",
  "timestamp": "2026-05-15T10:00:00Z",
  "type": "LOCK_COMMAND",
  "payload": {
    "action": "UNLOCK",
    "method": "MOBILE_KEY",
    "guest_id": "uuid",
    "valid_until": "2026-05-20T12:00:00Z"
  }
}
```

## 3. Integracije

### 3.1 Pametne brave
- Protokol: MQTT + Bluetooth Low Energy (BLE)
- Operacije: zakljucaj, otkljucaj, programiraj pristup, revociraj pristup
- Svaki pristup se loguje u bazu (audit trail)

### 3.2 Senzori prisutnosti
- Kada gost napusti sobu: automatski smanjiti temperaturu klime
- Kada gost udje: vratiti na postavljenu temperaturu
- Logika u `iot_services/` — backend dobija event, salje komandu HVAC-u

### 3.3 Energetska efikasnost
- Dashboard prikazuje potrosnju po sobi i po spratu
- Upozorenja: otvoren prozor dok klima radi
- Mjesecni izvjestaji energetske potrosnje

## 4. Restrikcije

- IoT servis MORA biti odvojen od glavnog backend API-ja
- MQTT broker MORA podrzavati TLS u produkciji
- Nijedan IoT uredjaj ne smije imati direktan pristup bazi — sve ide kroz API
- Svi IoT eventi moraju imati `device_id` i `timestamp`
- Backup scenario: ako MQTT padne, brave moraju nastaviti raditi lokalno (offline mode)
