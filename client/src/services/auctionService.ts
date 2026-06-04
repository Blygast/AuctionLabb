import http from './http';
import type { Auction, AuctionStatus, Bid } from '../types';

export interface CreateAuctionPayload {
  title: string;
  description: string;
  startingPrice: number;
  startDate: string; // ISO
  endDate: string;   // ISO
  files?: File[];
}

export interface UpdateAuctionPayload {
  title: string;
  description: string;
  endDate?: string;
}

export const auctionService = {
  async list(params: { search?: string; status?: AuctionStatus } = {}): Promise<Auction[]> {
    const res = await http.get<Auction[]>('/auctions', { params });
    return res.data;
  },

  async get(id: number): Promise<Auction> {
    const res = await http.get<Auction>(`/auctions/${id}`);
    return res.data;
  },

  async create(payload: CreateAuctionPayload): Promise<Auction> {
    const form = new FormData();
    form.append('Title', payload.title);
    form.append('Description', payload.description);
    form.append('StartingPrice', String(payload.startingPrice));
    form.append('StartDate', new Date(payload.startDate).toISOString());
    form.append('EndDate', new Date(payload.endDate).toISOString());
    payload.files?.forEach((f) => form.append('Files', f));
    const res = await http.post<Auction>('/auctions', form, {
      headers: { 'Content-Type': 'multipart/form-data' },
    });
    return res.data;
  },

  async update(id: number, payload: UpdateAuctionPayload): Promise<Auction> {
    const body: Record<string, unknown> = {
      title: payload.title,
      description: payload.description,
    };
    if (payload.endDate) body.endDate = new Date(payload.endDate).toISOString();
    const res = await http.put<Auction>(`/auctions/${id}`, body);
    return res.data;
  },
  async getBids(auctionId: number): Promise<Bid[]> {
    const res = await http.get<Bid[]>(`/auctions/${auctionId}/bids`);
    return res.data;
  },
  async placeBid(auctionId: number, amount: number): Promise<Bid> {
    const res = await http.post<Bid>(`/auctions/${auctionId}/bids`, { amount });
    return res.data;
  },
  async cancelBid(auctionId: number, bidId: number): Promise<void> {
    await http.delete(`/auctions/${auctionId}/bids/${bidId}`);
  },
};
