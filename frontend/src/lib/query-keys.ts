export const queryKeys = {
  tenants: {
    all: ["tenants"] as const,
    lists: () => [...queryKeys.tenants.all, "list"] as const,
    list: (filters: Record<string, string> = {}) =>
      [...queryKeys.tenants.lists(), filters] as const,
    details: () => [...queryKeys.tenants.all, "detail"] as const,
    detail: (id: string) => [...queryKeys.tenants.details(), id] as const,
  },
  users: {
    all: ["users"] as const,
    lists: () => [...queryKeys.users.all, "list"] as const,
    list: (filters: Record<string, string> = {}) =>
      [...queryKeys.users.lists(), filters] as const,
    details: () => [...queryKeys.users.all, "detail"] as const,
    detail: (id: string) => [...queryKeys.users.details(), id] as const,
  },
  roles: {
    all: ["roles"] as const,
    lists: () => [...queryKeys.roles.all, "list"] as const,
    list: (filters: Record<string, string> = {}) =>
      [...queryKeys.roles.lists(), filters] as const,
    detail: (id: string) => [...queryKeys.roles.all, "detail", id] as const,
  },
  customers: {
    all: ["customers"] as const,
    lists: () => [...queryKeys.customers.all, "list"] as const,
    list: (filters: Record<string, string> = {}) =>
      [...queryKeys.customers.lists(), filters] as const,
    details: () => [...queryKeys.customers.all, "detail"] as const,
    detail: (id: string) => [...queryKeys.customers.details(), id] as const,
    contacts: (id: string) => [...queryKeys.customers.detail(id), "contacts"] as const,
    notes: (id: string) => [...queryKeys.customers.detail(id), "notes"] as const,
    segments: (id: string) => [...queryKeys.customers.detail(id), "segments"] as const,
  },
  products: {
    all: ["products"] as const,
    lists: () => [...queryKeys.products.all, "list"] as const,
    list: (filters: Record<string, string> = {}) =>
      [...queryKeys.products.lists(), filters] as const,
    details: () => [...queryKeys.products.all, "detail"] as const,
    detail: (id: string) => [...queryKeys.products.details(), id] as const,
    offers: (id: string) => [...queryKeys.products.detail(id), "offers"] as const,
    configuration: (id: string) => [...queryKeys.products.detail(id), "configuration"] as const,
  },
  offers: {
    all: ["offers"] as const,
    lists: () => [...queryKeys.offers.all, "list"] as const,
    list: (filters: Record<string, string> = {}) =>
      [...queryKeys.offers.lists(), filters] as const,
    detail: (id: string) => [...queryKeys.offers.all, "detail", id] as const,
  },
  orders: {
    all: ["orders"] as const,
    lists: () => [...queryKeys.orders.all, "list"] as const,
    list: (filters: Record<string, string> = {}) =>
      [...queryKeys.orders.lists(), filters] as const,
    details: () => [...queryKeys.orders.all, "detail"] as const,
    detail: (id: string) => [...queryKeys.orders.details(), id] as const,
    fulfillment: (id: string) => [...queryKeys.orders.detail(id), "fulfillment"] as const,
    timeline: (id: string) => [...queryKeys.orders.detail(id), "timeline"] as const,
  },
  subscriptions: {
    all: ["subscriptions"] as const,
    lists: () => [...queryKeys.subscriptions.all, "list"] as const,
    list: (filters: Record<string, string> = {}) =>
      [...queryKeys.subscriptions.lists(), filters] as const,
    details: () => [...queryKeys.subscriptions.all, "detail"] as const,
    detail: (id: string) => [...queryKeys.subscriptions.details(), id] as const,
    entitlements: (id: string) => [...queryKeys.subscriptions.detail(id), "entitlements"] as const,
    usage: (id: string) => [...queryKeys.subscriptions.detail(id), "usage"] as const,
  },
  billing: {
    all: ["billing"] as const,
    bills: {
      all: ["billing", "bills"] as const,
      lists: () => [...queryKeys.billing.bills.all, "list"] as const,
      list: (filters: Record<string, string> = {}) =>
        [...queryKeys.billing.bills.lists(), filters] as const,
      detail: (id: string) => [...queryKeys.billing.bills.all, "detail", id] as const,
      adjustments: (id: string) => [...queryKeys.billing.bills.detail(id), "adjustments"] as const,
    },
    cycles: {
      all: ["billing", "cycles"] as const,
      lists: () => [...queryKeys.billing.cycles.all, "list"] as const,
      list: (filters: Record<string, string> = {}) =>
        [...queryKeys.billing.cycles.lists(), filters] as const,
      detail: (id: string) => [...queryKeys.billing.cycles.all, "detail", id] as const,
    },
    jobs: {
      all: ["billing", "jobs"] as const,
      lists: () => [...queryKeys.billing.jobs.all, "list"] as const,
      list: (filters: Record<string, string> = {}) =>
        [...queryKeys.billing.jobs.lists(), filters] as const,
      detail: (id: string) => [...queryKeys.billing.jobs.all, "detail", id] as const,
    },
    taxRules: {
      all: ["billing", "tax-rules"] as const,
      list: (filters: Record<string, string> = {}) =>
        [...queryKeys.billing.taxRules.all, "list", filters] as const,
    },
  },
  invoices: {
    all: ["invoices"] as const,
    lists: () => [...queryKeys.invoices.all, "list"] as const,
    list: (filters: Record<string, string> = {}) =>
      [...queryKeys.invoices.lists(), filters] as const,
    details: () => [...queryKeys.invoices.all, "detail"] as const,
    detail: (id: string) => [...queryKeys.invoices.details(), id] as const,
    disputes: {
      all: ["invoices", "disputes"] as const,
      list: (filters: Record<string, string> = {}) =>
        [...queryKeys.invoices.disputes.all, "list", filters] as const,
    },
    creditNotes: {
      all: ["invoices", "credit-notes"] as const,
      list: (filters: Record<string, string> = {}) =>
        [...queryKeys.invoices.creditNotes.all, "list", filters] as const,
    },
  },
  payments: {
    all: ["payments"] as const,
    lists: () => [...queryKeys.payments.all, "list"] as const,
    list: (filters: Record<string, string> = {}) =>
      [...queryKeys.payments.lists(), filters] as const,
    details: () => [...queryKeys.payments.all, "detail"] as const,
    detail: (id: string) => [...queryKeys.payments.details(), id] as const,
    reconciliation: {
      all: ["payments", "reconciliation"] as const,
      list: (filters: Record<string, string> = {}) =>
        [...queryKeys.payments.reconciliation.all, "list", filters] as const,
    },
    refunds: {
      all: ["payments", "refunds"] as const,
      list: (filters: Record<string, string> = {}) =>
        [...queryKeys.payments.refunds.all, "list", filters] as const,
    },
  },
  tickets: {
    all: ["tickets"] as const,
    lists: () => [...queryKeys.tickets.all, "list"] as const,
    list: (filters: Record<string, string> = {}) =>
      [...queryKeys.tickets.lists(), filters] as const,
    details: () => [...queryKeys.tickets.all, "detail"] as const,
    detail: (id: string) => [...queryKeys.tickets.details(), id] as const,
    sla: (id: string) => [...queryKeys.tickets.detail(id), "sla"] as const,
    slaDefinitions: () => [...queryKeys.tickets.all, "sla-definitions"] as const,
    comments: (id: string) => [...queryKeys.tickets.detail(id), "comments"] as const,
  },
  services: {
    all: ["services"] as const,
    lists: () => [...queryKeys.services.all, "list"] as const,
    list: (filters: Record<string, string> = {}) =>
      [...queryKeys.services.lists(), filters] as const,
    detail: (id: string) => [...queryKeys.services.all, "detail", id] as const,
    topology: (id: string) => [...queryKeys.services.detail(id), "topology"] as const,
  },
  networks: {
    all: ["network"] as const,
    elements: {
      all: ["network", "elements"] as const,
      list: (filters: Record<string, string> = {}) =>
        [...queryKeys.networks.elements.all, "list", filters] as const,
      detail: (id: string) => [...queryKeys.networks.elements.all, "detail", id] as const,
      connections: (id: string) => [...queryKeys.networks.elements.all, "detail", id, "connections"] as const,
      capacity: (id: string) => [...queryKeys.networks.elements.all, "detail", id, "capacity"] as const,
    },
    olts: {
      all: ["network", "olts"] as const,
      list: (filters: Record<string, string> = {}) =>
        [...queryKeys.networks.olts.all, "list", filters] as const,
      detail: (id: string) => [...queryKeys.networks.olts.all, "detail", id] as const,
      ports: (id: string) => [...queryKeys.networks.olts.all, "detail", id, "ports"] as const,
      registerOnt: (id: string) => [...queryKeys.networks.olts.all, "detail", id, "register-ont"] as const,
    },
    vlans: {
      all: ["network", "vlans"] as const,
      list: (filters: Record<string, string> = {}) =>
        [...queryKeys.networks.vlans.all, "list", filters] as const,
      detail: (id: string) => [...queryKeys.networks.vlans.all, "detail", id] as const,
    },
    links: {
      all: ["network", "links"] as const,
      lists: () => [...queryKeys.networks.links.all, "list"] as const,
      list: (filters: Record<string, string> = {}) =>
        [...queryKeys.networks.links.lists(), filters] as const,
      details: () => [...queryKeys.networks.links.all, "detail"] as const,
      detail: (id: string) => [...queryKeys.networks.links.details(), id] as const,
      degraded: () => [...queryKeys.networks.links.all, "degraded"] as const,
    },
    topology: {
      all: ["network", "topology"] as const,
      list: () => [...queryKeys.networks.topology.all, "list"] as const,
      maps: {
        all: ["network", "topology", "maps"] as const,
        list: () => [...queryKeys.networks.topology.maps.all, "list"] as const,
      },
    },
    subnets: {
      all: ["network", "subnets"] as const,
      lists: () => [...queryKeys.networks.subnets.all, "list"] as const,
      list: (filters: Record<string, string> = {}) =>
        [...queryKeys.networks.subnets.lists(), filters] as const,
      details: () => [...queryKeys.networks.subnets.all, "detail"] as const,
      detail: (id: string) => [...queryKeys.networks.subnets.details(), id] as const,
    },
    capacity: {
      all: ["network", "capacity"] as const,
      overview: () => [...queryKeys.networks.capacity.all, "overview"] as const,
      alerts: {
        all: ["network", "capacity", "alerts"] as const,
        list: () => [...queryKeys.networks.capacity.alerts.all, "list"] as const,
      },
    },
  },
  provisioning: {
    all: ["provisioning"] as const,
    jobs: {
      all: ["provisioning", "jobs"] as const,
      list: (filters: Record<string, string> = {}) =>
        [...queryKeys.provisioning.jobs.all, "list", filters] as const,
      detail: (id: string) => [...queryKeys.provisioning.jobs.all, "detail", id] as const,
      logs: (id: string) => [...queryKeys.provisioning.jobs.all, "detail", id, "logs"] as const,
    },
    templates: {
      all: ["provisioning", "templates"] as const,
      list: (filters: Record<string, string> = {}) =>
        [...queryKeys.provisioning.templates.all, "list", filters] as const,
      detail: (id: string) => [...queryKeys.provisioning.templates.all, "detail", id] as const,
    },
  },
  serviceInventory: {
    all: ["service-inventory"] as const,
    services: {
      all: ["service-inventory", "services"] as const,
      list: (filters: Record<string, string> = {}) =>
        [...queryKeys.serviceInventory.services.all, "list", filters] as const,
      detail: (id: string) => [...queryKeys.serviceInventory.services.all, "detail", id] as const,
      topology: (id: string) => [...queryKeys.serviceInventory.services.all, "detail", id, "topology"] as const,
      resources: (id: string) => [...queryKeys.serviceInventory.services.all, "detail", id, "resources"] as const,
    },
    discovery: {
      all: ["service-inventory", "discovery"] as const,
      list: (filters: Record<string, string> = {}) =>
        [...queryKeys.serviceInventory.discovery.all, "list", filters] as const,
    },
  },
  workflow: {
    all: ["workflow"] as const,
    definitions: {
      all: ["workflow", "definitions"] as const,
      list: (filters: Record<string, string> = {}) =>
        [...queryKeys.workflow.definitions.all, "list", filters] as const,
      detail: (id: string) => [...queryKeys.workflow.definitions.all, "detail", id] as const,
    },
    instances: {
      all: ["workflow", "instances"] as const,
      list: (filters: Record<string, string> = {}) =>
        [...queryKeys.workflow.instances.all, "list", filters] as const,
      detail: (id: string) => [...queryKeys.workflow.instances.all, "detail", id] as const,
    },
    slas: {
      all: ["workflow", "slas"] as const,
      list: () => [...queryKeys.workflow.slas.all, "list"] as const,
      detail: (id: string) => [...queryKeys.workflow.slas.all, "detail", id] as const,
    },
  },
  notifications: {
    all: ["notifications"] as const,
    list: (filters: Record<string, string> = {}) =>
      [...queryKeys.notifications.all, "list", filters] as const,
    templates: {
      all: ["notifications", "templates"] as const,
      lists: () => [...queryKeys.notifications.templates.all, "list"] as const,
      list: (filters: Record<string, string> = {}) =>
        [...queryKeys.notifications.templates.lists(), filters] as const,
      details: () => [...queryKeys.notifications.templates.all, "detail"] as const,
      detail: (id: string) => [...queryKeys.notifications.templates.details(), id] as const,
    },
  },
  reporting: {
    all: ["reporting"] as const,
    dashboard: () => [...queryKeys.reporting.all, "dashboard"] as const,
    definitions: {
      all: ["reporting", "definitions"] as const,
      list: () => [...queryKeys.reporting.definitions.all, "list"] as const,
    },
    schedules: {
      all: ["reporting", "schedules"] as const,
      list: () => [...queryKeys.reporting.schedules.all, "list"] as const,
    },
  },
  collections: {
    all: ["collections"] as const,
    cases: {
      all: ["collections", "cases"] as const,
      lists: () => [...queryKeys.collections.cases.all, "list"] as const,
      list: (filters: Record<string, string> = {}) =>
        [...queryKeys.collections.cases.lists(), filters] as const,
      detail: (id: string) => [...queryKeys.collections.cases.all, "detail", id] as const,
      actions: (id: string) => [...queryKeys.collections.cases.detail(id), "actions"] as const,
      arrangements: (id: string) => [...queryKeys.collections.cases.detail(id), "arrangements"] as const,
    },
    reports: {
      all: ["collections", "reports"] as const,
      aging: () => [...queryKeys.collections.reports.all, "aging"] as const,
    },
  },
  audit: {
    all: ["audit"] as const,
    entries: {
      all: ["audit", "entries"] as const,
      list: (filters: Record<string, string> = {}) =>
        [...queryKeys.audit.entries.all, "list", filters] as const,
    },
    entity: (entityType: string, entityId: string) =>
      ["audit", "entity", entityType, entityId] as const,
  },
  segments: {
    all: ["segments"] as const,
    lists: () => [...queryKeys.segments.all, "list"] as const,
    list: (filters: Record<string, string> = {}) =>
      [...queryKeys.segments.lists(), filters] as const,
    details: () => [...queryKeys.segments.all, "detail"] as const,
    detail: (id: string) => [...queryKeys.segments.details(), id] as const,
  },
  rating: {
    all: ["rating"] as const,
    rules: {
      all: ["rating", "rules"] as const,
      lists: () => [...queryKeys.rating.rules.all, "list"] as const,
      list: (filters: Record<string, string> = {}) =>
        [...queryKeys.rating.rules.lists(), filters] as const,
      details: () => [...queryKeys.rating.rules.all, "detail"] as const,
      detail: (id: string) => [...queryKeys.rating.rules.details(), id] as const,
    },
    promotions: {
      all: ["rating", "promotions"] as const,
      lists: () => [...queryKeys.rating.promotions.all, "list"] as const,
      list: (filters: Record<string, string> = {}) =>
        [...queryKeys.rating.promotions.lists(), filters] as const,
      details: () => [...queryKeys.rating.promotions.all, "detail"] as const,
      detail: (id: string) => [...queryKeys.rating.promotions.details(), id] as const,
      applicable: (params: Record<string, string> = {}) =>
        [...queryKeys.rating.promotions.all, "applicable", params] as const,
    },
    usage: {
      all: ["rating", "usage"] as const,
      lists: () => [...queryKeys.rating.usage.all, "list"] as const,
      list: (filters: Record<string, string> = {}) =>
        [...queryKeys.rating.usage.lists(), filters] as const,
      details: () => [...queryKeys.rating.usage.all, "detail"] as const,
      detail: (id: string) => [...queryKeys.rating.usage.details(), id] as const,
      unrated: ["rating", "usage", "unrated"] as const,
      bySubscription: (subscriptionId: string, filters: Record<string, string> = {}) =>
        [...queryKeys.rating.usage.all, "subscription", subscriptionId, filters] as const,
    },
  },
  numberInventory: {
    all: ["number-inventory"] as const,
    numbers: {
      all: ["number-inventory", "numbers"] as const,
      lists: () => [...queryKeys.numberInventory.numbers.all, "list"] as const,
      list: (filters: Record<string, string> = {}) =>
        [...queryKeys.numberInventory.numbers.lists(), filters] as const,
      available: (filters: Record<string, string> = {}) =>
        [...queryKeys.numberInventory.numbers.all, "available", filters] as const,
      details: () => [...queryKeys.numberInventory.numbers.all, "detail"] as const,
      detail: (id: string) => [...queryKeys.numberInventory.numbers.details(), id] as const,
    },
  },
  productSpecifications: {
    all: ["product-specifications"] as const,
    lists: () => [...queryKeys.productSpecifications.all, "list"] as const,
    list: (filters: Record<string, string> = {}) =>
      [...queryKeys.productSpecifications.lists(), filters] as const,
    details: () => [...queryKeys.productSpecifications.all, "detail"] as const,
    detail: (id: string) => [...queryKeys.productSpecifications.details(), id] as const,
  },
  gateway: {
    all: ["gateway"] as const,
    routes: {
      all: ["gateway", "routes"] as const,
      list: () => [...queryKeys.gateway.routes.all, "list"] as const,
    },
    apiKeys: {
      all: ["gateway", "api-keys"] as const,
      list: () => [...queryKeys.gateway.apiKeys.all, "list"] as const,
    },
    partners: {
      all: ["gateway", "partners"] as const,
      list: () => [...queryKeys.gateway.partners.all, "list"] as const,
    },
  },
} as const
