'use client';

import { useQuote } from '@/api/hooks/useQuotes';
import { Card } from '@/components/ui/card';
import { Badge } from '@/components/ui/badge';
import { use } from 'react';

const statusColors: Record<string, string> = {
  InProgress: 'bg-blue-100 text-blue-800',
  Pending: 'bg-yellow-100 text-yellow-800',
  Approved: 'bg-green-100 text-green-800',
  Accepted: 'bg-emerald-100 text-emerald-800',
  Rejected: 'bg-red-100 text-red-800',
  Cancelled: 'bg-gray-100 text-gray-800',
};

export default function QuoteDetailPage({ params }: { params: Promise<{ id: string }> }) {
  const { id } = use(params);
  const { data: quote, isLoading } = useQuote(id);

  if (isLoading) return <div>Loading...</div>;
  if (!quote) return <div>Quote not found</div>;

  return (
    <div className="space-y-6">
      <div className="flex justify-between items-start">
        <div>
          <h1 className="text-2xl font-bold">{quote.externalId ?? `Quote #${quote.id.slice(0, 8)}`}</h1>
          <p className="text-sm text-gray-500">Version {quote.version}</p>
        </div>
        <Badge className={statusColors[quote.state]}>{quote.state}</Badge>
      </div>

      {quote.description && <p className="text-gray-600">{quote.description}</p>}

      <div className="grid grid-cols-2 gap-4">
        <div>
          <h3 className="font-semibold">Details</h3>
          <dl className="text-sm space-y-1 mt-1">
            <dt className="text-gray-500">Customer ID</dt>
            <dd>{quote.customerId}</dd>
            {quote.category && <><dt className="text-gray-500">Category</dt><dd>{quote.category}</dd></>}
            {quote.validFrom && <><dt className="text-gray-500">Valid From</dt><dd>{new Date(quote.validFrom).toLocaleDateString()}</dd></>}
            {quote.validUntil && <><dt className="text-gray-500">Valid Until</dt><dd>{new Date(quote.validUntil).toLocaleDateString()}</dd></>}
            <dt className="text-gray-500">Created</dt>
            <dd>{new Date(quote.createdAt).toLocaleString()}</dd>
          </dl>
        </div>
        <div>
          <h3 className="font-semibold">Dates</h3>
          <dl className="text-sm space-y-1 mt-1">
            <dt className="text-gray-500">Quote Date</dt>
            <dd>{new Date(quote.quoteDate).toLocaleString()}</dd>
            {quote.expectedQuoteCompletionDate && <><dt className="text-gray-500">Expected Completion</dt><dd>{new Date(quote.expectedQuoteCompletionDate).toLocaleDateString()}</dd></>}
            {quote.effectiveQuoteCompletionDate && <><dt className="text-gray-500">Effective Completion</dt><dd>{new Date(quote.effectiveQuoteCompletionDate).toLocaleDateString()}</dd></>}
            {quote.expectedFulfillmentStartDate && <><dt className="text-gray-500">Expected Fulfillment</dt><dd>{new Date(quote.expectedFulfillmentStartDate).toLocaleDateString()}</dd></>}
          </dl>
        </div>
      </div>

      {quote.items.length > 0 && (
        <section>
          <h2 className="text-lg font-semibold">Items</h2>
          <div className="space-y-2 mt-2">
            {quote.items.map(item => (
              <Card key={item.id} className="p-3">
                <div className="flex justify-between items-start">
                  <div>
                    <p className="font-medium">{item.productOfferingName ?? `Item #${item.id.slice(0, 8)}`}</p>
                    <p className="text-sm text-gray-500">Action: {item.action} | Qty: {item.quantity}</p>
                  </div>
                  <Badge>{item.state}</Badge>
                </div>
                {item.prices.map((price, idx) => (
                  <p key={idx} className="text-sm mt-1">
                    {price.name}: {price.dutyFreeAmount} {price.currency}
                  </p>
                ))}
              </Card>
            ))}
          </div>
        </section>
      )}

      {quote.quotePrices.length > 0 && (
        <section>
          <h2 className="text-lg font-semibold">Pricing Summary</h2>
          <div className="space-y-1 mt-1">
            {quote.quotePrices.map((price, idx) => (
              <p key={idx} className="text-sm">
                {price.name}: {price.dutyFreeAmount} {price.currency} ({price.priceType})
              </p>
            ))}
          </div>
        </section>
      )}
    </div>
  );
}