'use client';

import { useQuotes } from '@/api/hooks/useQuotes';
import { Card } from '@/components/ui/card';
import { Badge } from '@/components/ui/badge';
import Link from 'next/link';

const statusColors: Record<string, string> = {
  InProgress: 'bg-blue-100 text-blue-800',
  Pending: 'bg-yellow-100 text-yellow-800',
  Approved: 'bg-green-100 text-green-800',
  Accepted: 'bg-emerald-100 text-emerald-800',
  Rejected: 'bg-red-100 text-red-800',
  Cancelled: 'bg-gray-100 text-gray-800',
};

export default function QuotesPage() {
  const { data: quotes, isLoading } = useQuotes();

  if (isLoading) return <div>Loading...</div>;

  return (
    <div className="space-y-4">
      <h1 className="text-2xl font-bold">Quotes</h1>
      <div className="grid gap-4">
        {quotes?.map(quote => (
          <Link key={quote.id} href={`/crm/quotes/${quote.id}`}>
            <Card className="p-4 hover:shadow-md transition-shadow">
              <div className="flex justify-between items-start">
                <div>
                  <h3 className="font-semibold">{quote.externalId ?? `Quote #${quote.id.slice(0, 8)}`}</h3>
                  <p className="text-sm text-gray-500">Customer: {quote.customerId}</p>
                  {quote.category && <p className="text-sm text-gray-500">Category: {quote.category}</p>}
                </div>
                <Badge className={statusColors[quote.state]}>{quote.state}</Badge>
              </div>
              {quote.description && (
                <p className="text-sm mt-2 text-gray-600">{quote.description}</p>
              )}
              <div className="flex gap-2 mt-2 text-xs text-gray-400">
                <span>v{quote.version}</span>
                <span>{quote.items.length} item(s)</span>
                <span>{quote.quotePrices.length} price(s)</span>
              </div>
            </Card>
          </Link>
        ))}
        {quotes?.length === 0 && <p className="text-gray-500">No quotes found.</p>}
      </div>
    </div>
  );
}