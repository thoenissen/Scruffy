import { participant } from "./participant"

export interface appointment {
    id: number,
    timestamp: Date,
    name: string,
    participants: participant[]
}