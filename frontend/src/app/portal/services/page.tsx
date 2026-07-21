"use client"

import { useQuery } from "@tanstack/react-query"
import { Card, CardContent, CardHeader, CardTitle, CardDescription } from "@/components/ui/card"
import { Badge } from "@/components/ui/badge"
import { Button } from "@/components/ui/button"
import { Skeleton } from "@/components/ui/skeleton"
import { Wifi, Tv, Phone, Globe } from "lucide-react"

async function fetchServices() {
  const res = await fetch("/api/v1/customers/me/services")
  if (!res.ok) throw new Error("Failed to load services")
  return res.json()
}

const serviceIcons: Record<string, React.ReactNode> = {
  internet: <Wifi className="h-5 w-5" />,
  iptv: <Tv className="h-5 w-5" />,
  voip: <Phone className="h-5 w-5" />,
  hosting: <Globe className="h-5 w-5" />,
}

export default function PortalServicesPage() {
  const { data: services, isLoading } = useQuery({
    queryKey: ["portal-services"],
    queryFn: fetchServices,
  })

  return (
    <div className="space-y-6">
      <div>
        <h1 className="text-2xl font-bold">My Services</h1>
        <p className="text-muted-foreground">View and manage your subscribed services</p>
      </div>

      {isLoading ? (
        <div className="grid gap-4 md:grid-cols-2">
          {[1, 2].map((i) => (
            <Card key={i}><CardHeader><Skeleton className="h-5 w-32" /><Skeleton className="h-4 w-48" /></CardHeader></Card>
          ))}
        </div>
      ) : (
        <div className="grid gap-4 md:grid-cols-2">
          {services?.length > 0 ? services.map((service: any) => (
            <Card key={service.id}>
              <CardHeader className="flex flex-row items-start justify-between">
                <div>
                  <CardTitle className="flex items-center gap-2">
                    {serviceIcons[service.serviceType?.toLowerCase()] || <Globe className="h-5 w-5" />}
                    {service.name}
                  </CardTitle>
                  <CardDescription>{service.description}</CardDescription>
                </div>
                <Badge variant={service.status === "Active" ? "default" : "secondary"}>{service.status}</Badge>
              </CardHeader>
              <CardContent>
                <div className="flex items-center justify-between">
                  <span className="text-sm text-muted-foreground">{service.serviceType}</span>
                  <Button variant="outline" size="sm">Manage</Button>
                </div>
              </CardContent>
            </Card>
          )) : (
            <p className="col-span-2 text-sm text-muted-foreground">No active services found</p>
          )}
        </div>
      )}
    </div>
  )
}
