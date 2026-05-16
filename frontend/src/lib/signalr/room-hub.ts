import * as signalR from '@microsoft/signalr'

type StatusChangeCallback = (data: {
  roomId: string
  roomNumber: string
  oldStatus: string
  newStatus: string
  timestamp: string
}) => void

class RoomHubService {
  private connection: signalR.HubConnection | null = null
  private callbacks: StatusChangeCallback[] = []
  private reconnectDelays = [0, 2000, 5000, 10000, 30000]

  async connect(apiUrl: string, token: string) {
    this.connection = new signalR.HubConnectionBuilder()
      .withUrl(`${apiUrl}/hubs/room-status`, {
        accessTokenFactory: () => token,
      })
      .withAutomaticReconnect(this.reconnectDelays)
      .build()

    this.connection.on('RoomStatusChanged', (data: {
      roomId: string
      roomNumber: string
      oldStatus: string
      newStatus: string
      timestamp: string
    }) => {
      this.callbacks.forEach((cb) => cb(data))
    })

    await this.connection.start()
  }

  onStatusChange(callback: StatusChangeCallback): () => void {
    this.callbacks.push(callback)
    return () => {
      this.callbacks = this.callbacks.filter((c) => c !== callback)
    }
  }

  async disconnect() {
    await this.connection?.stop()
    this.connection = null
  }
}

export const roomHubService = new RoomHubService()
