'use client';

import { useProductInventoryList } from '@/api/hooks/useProductInventory';
import { Card } from '@/components/ui/card';
import { Badge } from '@/components/ui/badge';
import Link from 'next/link';

const statusColors: Record<string, string> = {
  Created: 'bg-gray-100 text-gray-800',
  Active: 'bg-green-100 text-green-800',
  Suspended: 'bg-yellow-100 text-yellow-800',
  Cancelled: 'bg-red-100 text-red-800',
  Terminated: 'bg-gray-100 text-gray-800',
};

export default function ProductsPage() {
  const { data: products, isLoading } = useProductInventoryList();

  if (isLoading) return <div>Loading...</div>;

  return (
    <div className="space-y-4">
      <h1 className="text-2xl font-bold">Product Inventory</h1>
      <div className="grid gap-4">
        {products?.map(product => (
          <Link key={product.id} href={`/subscriptions/products/${product.id}`}>
            <Card className="p-4 hover:shadow-md transition-shadow">
              <div className="flex justify-between items-start">
                <div>
                  <h3 className="font-semibold">{product.name ?? 'Unnamed Product'}</h3>
                  <p className="text-sm text-gray-500">Customer: {product.customerId}</p>
                </div>
                <Badge className={statusColors[product.status]}>{product.status}</Badge>
              </div>
              {product.description && (
                <p className="text-sm mt-2 text-gray-600">{product.description}</p>
              )}
              <div className="flex gap-2 mt-2 text-xs text-gray-400">
                <span>{product.prices.length} price(s)</span>
                <span>{product.terms.length} term(s)</span>
                <span>{product.realizingServices.length} service(s)</span>
              </div>
            </Card>
          </Link>
        ))}
        {products?.length === 0 && <p className="text-gray-500">No products found.</p>}
      </div>
    </div>
  );
}
