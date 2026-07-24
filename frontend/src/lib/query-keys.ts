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
  productInventory: {
    all: ["product-inventory"] as const,
    lists: () => [...queryKeys.productInventory.all, "list"] as const,
    list: (filters: Record<string, string> = {}) =>
      [...queryKeys.productInventory.lists(), filters] as const,
    details: () => [...queryKeys.productInventory.all, "detail"] as const,
    detail: (id: string) => [...queryKeys.productInventory.details(), id] as const,
  },
  offers: {
    all: ["offers"] as const,
    lists: () => [...queryKeys.offers.all, "list"] as const,
    list: (filters: Record<string, string> = {}) =>
      [...queryKeys.offers.lists(), filters] as const,
    detail: (id: string) => [...queryKeys.offers.all, "detail", id] as const,
  },
  orders: {
    all: ["orders", "product-orders"] as const,
    lists: () => [...queryKeys.orders.all, "list"] as const,
    list: (filters: Record<string, string> = {}) =>
      [...queryKeys.orders.lists(), filters] as const,
    details: () => [...queryKeys.orders.all, "detail"] as const,
    detail: (id: string) => [...queryKeys.orders.details(), id] as const,
    relationships: (id: string) => [...queryKeys.orders.detail(id), "relationships"] as const,
    milestones: (id: string) => [...queryKeys.orders.detail(id), "milestones"] as const,
    timeline: (id: string) => [...queryKeys.orders.detail(id), "timeline"] as const,
    fulfillment: (id: string) => [...queryKeys.orders.detail(id), "fulfillment"] as const,
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
    billingAccounts: {
      all: ["billing", "billing-accounts"] as const,
      lists: () => [...queryKeys.billing.billingAccounts.all, "list"] as const,
      list: (filters: Record<string, string> = {}) =>
        [...queryKeys.billing.billingAccounts.lists(), filters] as const,
      details: () => [...queryKeys.billing.billingAccounts.all, "detail"] as const,
      detail: (id: string) => [...queryKeys.billing.billingAccounts.details(), id] as const,
      balance: (id: string) => [...queryKeys.billing.billingAccounts.detail(id), "balance"] as const,
      relatedParties: (id: string) => [...queryKeys.billing.billingAccounts.detail(id), "related-parties"] as const,
      presentationMedia: (id: string) => [...queryKeys.billing.billingAccounts.detail(id), "presentation-media"] as const,
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
    summary: ["payments", "summary"] as const,
    byInvoice: (invoiceId: string) => ["payments", "by-invoice", invoiceId] as const,
    gateways: {
      all: ["payments", "gateways"] as const,
      list: (filters: Record<string, string> = {}) =>
        [...queryKeys.payments.gateways.all, "list", filters] as const,
    },
    unmatched: ["payments", "unmatched"] as const,
    reconciliation: {
      all: ["payments", "reconciliation"] as const,
      list: (filters: Record<string, string> = {}) =>
        [...queryKeys.payments.reconciliation.all, "list", filters] as const,
      detail: (id: string) => [...queryKeys.payments.reconciliation.all, "detail", id] as const,
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
    dunningPolicies: {
      all: ["collections", "dunning-policies"] as const,
      lists: () => [...queryKeys.collections.dunningPolicies.all, "list"] as const,
      list: (filters: Record<string, string> = {}) =>
        [...queryKeys.collections.dunningPolicies.lists(), filters] as const,
      detail: (id: string) => [...queryKeys.collections.dunningPolicies.all, "detail", id] as const,
    },
    dashboard: () => ["collections", "dashboard"] as const,
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
  quotes: {
    all: ["quotes"] as const,
    lists: () => [...queryKeys.quotes.all, "list"] as const,
    list: (filters: Record<string, string> = {}) =>
      [...queryKeys.quotes.lists(), filters] as const,
    details: () => [...queryKeys.quotes.all, "detail"] as const,
    detail: (id: string) => [...queryKeys.quotes.details(), id] as const,
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
  serviceCatalog: {
    all: ["service-catalog"] as const,
    categories: {
      all: ["service-catalog", "categories"] as const,
      lists: () => [...queryKeys.serviceCatalog.categories.all, "list"] as const,
      list: (filters: Record<string, string> = {}) => [...queryKeys.serviceCatalog.categories.lists(), filters] as const,
      details: () => [...queryKeys.serviceCatalog.categories.all, "detail"] as const,
      detail: (id: string) => [...queryKeys.serviceCatalog.categories.details(), id] as const,
    },
    candidates: {
      all: ["service-catalog", "candidates"] as const,
      lists: () => [...queryKeys.serviceCatalog.candidates.all, "list"] as const,
      list: (filters: Record<string, string> = {}) => [...queryKeys.serviceCatalog.candidates.lists(), filters] as const,
      details: () => [...queryKeys.serviceCatalog.candidates.all, "detail"] as const,
      detail: (id: string) => [...queryKeys.serviceCatalog.candidates.details(), id] as const,
    },
    specifications: {
      all: ["service-catalog", "specifications"] as const,
      lists: () => [...queryKeys.serviceCatalog.specifications.all, "list"] as const,
      list: (filters: Record<string, string> = {}) => [...queryKeys.serviceCatalog.specifications.lists(), filters] as const,
      details: () => [...queryKeys.serviceCatalog.specifications.all, "detail"] as const,
      detail: (id: string) => [...queryKeys.serviceCatalog.specifications.details(), id] as const,
    },
    characteristics: {
      all: (specId: string) => ["service-catalog", "specifications", specId, "characteristics"] as const,
      list: (specId: string) => [...queryKeys.serviceCatalog.characteristics.all(specId), "list"] as const,
    },
    characteristicValues: {
      all: (specId: string, charId: string) => ["service-catalog", "specifications", specId, "characteristics", charId, "values"] as const,
      list: (specId: string, charId: string) => [...queryKeys.serviceCatalog.characteristicValues.all(specId, charId), "list"] as const,
    },
    relationships: {
      all: (specId: string) => ["service-catalog", "specifications", specId, "relationships"] as const,
      list: (specId: string) => [...queryKeys.serviceCatalog.relationships.all(specId), "list"] as const,
    },
  },
  serviceQualification: {
    all: ["service-qualification"] as const,
    lists: () => [...queryKeys.serviceQualification.all, "list"] as const,
    list: (filters: Record<string, string> = {}) =>
      [...queryKeys.serviceQualification.lists(), filters] as const,
    details: () => [...queryKeys.serviceQualification.all, "detail"] as const,
    detail: (id: string) => [...queryKeys.serviceQualification.details(), id] as const,
  },
  ocs: {
    all: ["ocs"] as const,
    balances: {
      all: () => [...queryKeys.ocs.all, "balances"] as const,
      lists: () => [...queryKeys.ocs.balances.all(), "list"] as const,
      list: (filters: Record<string, string> = {}) =>
        [...queryKeys.ocs.balances.lists(), filters] as const,
      details: () => [...queryKeys.ocs.balances.all(), "detail"] as const,
      detail: (id: string) => [...queryKeys.ocs.balances.details(), id] as const,
    },
    creditPools: {
      all: () => [...queryKeys.ocs.all, "credit-pools"] as const,
      lists: () => [...queryKeys.ocs.creditPools.all(), "list"] as const,
      list: (filters: Record<string, string> = {}) =>
        [...queryKeys.ocs.creditPools.lists(), filters] as const,
      details: () => [...queryKeys.ocs.creditPools.all(), "detail"] as const,
      detail: (id: string) => [...queryKeys.ocs.creditPools.details(), id] as const,
    },
    transactions: {
      all: () => [...queryKeys.ocs.all, "transactions"] as const,
      lists: () => [...queryKeys.ocs.transactions.all(), "list"] as const,
      list: (filters: Record<string, string> = {}) =>
        [...queryKeys.ocs.transactions.lists(), filters] as const,
    },
  },
  eventManagement: {
    all: ["event-management"] as const,
    subscriptions: {
      all: () => [...queryKeys.eventManagement.all, "subscriptions"] as const,
      lists: () => [...queryKeys.eventManagement.subscriptions.all(), "list"] as const,
      list: (filters: Record<string, string> = {}) =>
        [...queryKeys.eventManagement.subscriptions.lists(), filters] as const,
      details: () => [...queryKeys.eventManagement.subscriptions.all(), "detail"] as const,
      detail: (id: string) => [...queryKeys.eventManagement.subscriptions.details(), id] as const,
    },
    events: {
      all: () => [...queryKeys.eventManagement.all, "events"] as const,
      lists: () => [...queryKeys.eventManagement.events.all(), "list"] as const,
      list: (filters: Record<string, string> = {}) =>
        [...queryKeys.eventManagement.events.lists(), filters] as const,
    },
  },
  aaa: {
    metrics: () => ["aaa", "metrics"] as const,
    nas: {
      all: () => ["aaa", "nas"] as const,
      lists: () => [...queryKeys.aaa.nas.all(), "list"] as const,
      list: (filters: Record<string, string> = {}) =>
        [...queryKeys.aaa.nas.lists(), filters] as const,
      details: () => [...queryKeys.aaa.nas.all(), "detail"] as const,
      detail: (id: string) => [...queryKeys.aaa.nas.details(), id] as const,
    },
    sessions: {
      all: () => ["aaa", "sessions"] as const,
      lists: () => [...queryKeys.aaa.sessions.all(), "list"] as const,
      list: (filters: Record<string, string> = {}) =>
        [...queryKeys.aaa.sessions.lists(), filters] as const,
      details: () => [...queryKeys.aaa.sessions.all(), "detail"] as const,
      detail: (id: string) => [...queryKeys.aaa.sessions.details(), id] as const,
    },
    logs: {
      lists: () => ["aaa", "logs", "list"] as const,
      list: (filters: Record<string, string> = {}) =>
        [...queryKeys.aaa.logs.lists(), filters] as const,
    },
  },
} as const
