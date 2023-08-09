import { participant } from './participant';

export interface appointment {
  id: number;
  timestamp: Date;
  title: string;
  participants: participant[];
}
