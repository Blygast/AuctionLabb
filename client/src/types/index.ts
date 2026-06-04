export interface Attachment {
  id: number;
  fileName: string;
  contentType: string;
  fileSize: number;
  uploadedAt: string;
  url: string;
}

export interface Bid {
  id: number;
  amount: number;
  bidDate: string;
  userId: number;
  userName: string;
}

export interface Auction {
  id: number;
  title: string;
  description: string;
  startingPrice: number;
  startDate: string;
  endDate: string;
  isOpen: boolean;
  isActive: boolean;
  userId: number;
  userName: string;
  highestBid: number | null;
  attachments: Attachment[];
  bids: Bid[];
  winningBid?: Bid | null;
}

export interface AuthUser {
  userId: number;
  name: string;
  email: string;
  role: string;
}

export interface AuthResponse extends AuthUser {
  token: string;
}

export interface AdminUser {
  id: number;
  name: string;
  email: string;
  role: string;
  isActive: boolean;
  auctionCount: number;
  bidCount: number;
}

export interface AdminAuction {
  id: number;
  title: string;
  isActive: boolean;
  isOpen: boolean;
  startDate: string;
  endDate: string;
  startingPrice: number;
  userName: string;
  userId: number;
  bidCount: number;
  highestBid: number | null;
}

export type AuctionStatus = 'open' | 'closed' | 'all';
