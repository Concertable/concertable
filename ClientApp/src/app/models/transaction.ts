export interface Transaction {
    fromUserId: number;
    toUserId: number;
    transactionId: string;
    amount: number;
    type: string;
    status: string;
    createdAt: string; 
  }
  