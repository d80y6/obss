'use client';

import { useProductInventory } from '@/api/hooks/useProductInventory';
import { Card } from '@/components/ui/card';
import { Badge } from '@/components/ui/badge';
import { use } from 'react';

export default function ProductDetailPage({ params }: { params: Promise<{ id: string }> }) {
  const { id } = use(params);
  const { data: product, isLoading } = useProductInventory(id);

  if (isLoading) return <div>Loading...</div>;
  if (!product) return <div>Product not found</div>;

  return (
    <div className="space-y-6">
      <div>
        <h1 className="text-2xl font-bold">{product.name ?? 'Unnamed Product'}</h1>
        <Badge>{product.status}</Badge>
      </div>

      {product.description && <p className="text-gray-600">{product.description}</p>}

      {product.prices.length > 0 && (
        <section>
          <h2 className="text-lg font-semibold">Prices</h2>
          {product.prices.map(price => (
            <Card key={price.id} className="p-3 mt-2">
              <span className="font-medium">{price.name}</span>: {price.amount} {price.currency}
              <span className="text-gray-500 ml-2">({price.priceType})</span>
            </Card>
          ))}
        </section>
      )}

      {product.terms.length > 0 && (
        <section>
          <h2 className="text-lg font-semibold">Terms</h2>
          {product.terms.map(term => (
            <Card key={term.id} className="p-3 mt-2">
              <span className="font-medium">{term.name}</span> — {term.duration} {term.durationUnit}
            </Card>
          ))}
        </section>
      )}

      {product.realizingServices.length > 0 && (
        <section>
          <h2 className="text-lg font-semibold">Realizing Services</h2>
          {product.realizingServices.map(s => (
            <Card key={s.id} className="p-3 mt-2">
              Service {s.serviceId} — {s.serviceType} — {s.status}
            </Card>
          ))}
        </section>
      )}

      {product.realizingResources.length > 0 && (
        <section>
          <h2 className="text-lg font-semibold">Realizing Resources</h2>
          {product.realizingResources.map(r => (
            <Card key={r.id} className="p-3 mt-2">
              Resource {r.resourceId} — {r.resourceType} — {r.status}
            </Card>
          ))}
        </section>
      )}

      {product.characteristics.length > 0 && (
        <section>
          <h2 className="text-lg font-semibold">Characteristics</h2>
          {product.characteristics.map(c => (
            <Card key={c.id} className="p-3 mt-2">
              {c.name}: {c.value}
            </Card>
          ))}
        </section>
      )}
    </div>
  );
}
