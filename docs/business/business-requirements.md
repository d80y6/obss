# Business Requirements Document

## Telecom OSS/BSS Platform — Yemen Market

| Document ID | BRD-OSS-BSS-001 |
|---|---|
| Version | 1.0 |
| Status | Draft |
| Date | 2026-06-20 |
| Market | Republic of Yemen |

---

## Table of Contents

1. [Executive Summary](#1-executive-summary)
2. [Business Goals](#2-business-goals)
3. [Customer Segments](#3-customer-segments)
4. [Consumer Telecom Services](#4-consumer-telecom-services)
5. [Business Services](#5-business-services)
6. [Hosting & Digital Services](#6-hosting--digital-services)
7. [Revenue Requirements](#7-revenue-requirements)
8. [Billing Requirements](#8-billing-requirements)
9. [Provisioning Requirements](#9-provisioning-requirements)
10. [Customer Lifecycle](#10-customer-lifecycle)
11. [Order Lifecycle](#11-order-lifecycle)
12. [Payment Methods](#12-payment-methods)
13. [Business Rules](#13-business-rules)
14. [Glossary](#14-glossary)

---

## 1. Executive Summary

The Telecom OSS/BSS Platform is a greenfield digital operations and business support system for an ISP/Telco operating in the Republic of Yemen. The platform must support the full breadth of consumer and business telecommunications services, hosting and digital services, while addressing the unique constraints of the Yemeni market: dual-currency economy (YER/USD), limited banking infrastructure, intermittent power and connectivity, and a regulatory environment with no VAT but a 5% withholding tax regime.

The platform replaces fragmented legacy systems with a unified, modular, API-first architecture covering CRM, product catalog, ordering, subscription management, rating, billing, invoicing, payments, collections, provisioning, network inventory, workflow engine, ticketing, notifications, reporting, and audit.

---

## 2. Business Goals

### 2.1 Revenue Growth

| Goal | KPI | Target (Year 1) | Target (Year 3) |
|---|---|---|---|
| Grow top-line revenue | Monthly Recurring Revenue (MRR) | +15% | +50% |
| Increase ARPU | Average Revenue Per Unit | +10% | +25% |
| Reduce revenue leakage | Collection rate | >95% | >98% |
| Launch new services | Time-to-market for new offers | <2 weeks | <1 week |

**Strategic Initiatives:**
- Enable real-time credit control to reduce bad debt
- Support dynamic pricing and promotions to drive uptake
- Introduce loyalty programs to reduce churn
- Enable partner/wholesale billing for reseller channels

### 2.2 Market Share

| Goal | KPI | Target (Year 1) | Target (Year 3) |
|---|---|---|---|
| Expand residential fiber footprint | FTTH subscriber count | +20% | +60% |
| Grow business segment | Business customer count | +25% | +80% |
| Increase WiFi hotspot usage | Active Yemen WiFi users | +30% | +100% |

### 2.3 Operational Efficiency

| Goal | KPI | Target |
|---|---|---|
| Reduce order-to-activation time | Average time from order to service activation | <4 hours (residential), <24 hours (business) |
| Automate provisioning | Percentage of fully automated activations | >90% |
| Reduce billing inquiries | Billing-related tickets as % of total | <5% |
| First-call resolution | FCR rate | >80% |

### 2.4 Customer Experience

| Goal | KPI | Target |
|---|---|---|
| Improve CSAT | Customer Satisfaction Score | >4.0 / 5.0 |
| Reduce churn | Monthly churn rate | <1.5% |
| Self-service adoption | Portal/app login rate | >60% of active customers monthly |
| Omnichannel support | Channels supported | Web, mobile app, call center, WhatsApp, walk-in |

---

## 3. Customer Segments

### 3.1 Residential

**Demographics:** Household users in urban and semi-urban areas (Sana'a, Aden, Taiz, Hodeidah, Mukalla, Ibb, Dhamar).

**Characteristics:**
- Price-sensitive; YER-denominated billing preferred
- Prepaid-dominant culture due to banking trust issues
- Low banking penetration; cash and mobile money preferred
- Household sharing of internet connection
- Limited technical literacy; simplified plans preferred

**Services consumed:** FTTH, ADSL, Fixed Wireless, Yemen WiFi, Fixed Telephony (VoIP), Static IP.

**Churn drivers:** Price, service quality (uptime, speed), customer service responsiveness.

### 3.2 SMB (Small & Medium Business)

**Definition:** 1–50 employees. Retail shops, cafes, clinics, small offices, schools, NGOs.

**Characteristics:**
- Mix of YER and USD billing (USD for higher-tier services)
- Postpaid preferred for accounting/tax purposes
- Requires invoices for bookkeeping
- May need static IPs for CCTV, POS systems
- Price-sensitive but values reliability over lowest price

**Services consumed:** FTTH (business-grade), Fixed Wireless, Static IP, DIA (lower tiers), Hosting, Domain Registration, VoIP trunking.

### 3.3 Enterprise

**Definition:** >50 employees. Banks, telecoms, government entities, large NGOs, universities, hotels.

**Characteristics:**
- USD-denominated contracts preferred
- Postpaid with net-30 or net-60 terms
- Requires tax invoices with withholding tax deduction
- SLA guarantees mandatory (uptime, latency, MTTR)
- Dedicated account management
- Complex multi-service bundles

**Services consumed:** DIA, Ethernet, PRI, Dedicated Servers, Colocation, VPS, Static IP blocks, Domain Registration.

### 3.4 Wholesale

**Definition:** Other ISPs, carriers, and resellers purchasing capacity or services.

**Characteristics:**
- USD-denominated
- Net-30 or net-60 terms
- Usage-based billing (95th percentile, total GB)
- SLA with penalties
- Interconnection agreements
- Reseller white-label capabilities

**Services consumed:** DIA (high-capacity), Ethernet, Colocation, IP transit.

---

## 4. Consumer Telecom Services

### 4.1 FTTH (Fiber to the Home)

**Definition:** Fiber-optic broadband delivered directly to the subscriber's premises. GPON/XGS-PON architecture.

**Provisioning Requirements:**
- OLT port + ONT/ONU registration (serial number, MAC)
- GPON service profile configuration (bandwidth profile, VLAN, QoS)
- ONT activation at customer premises
- Speed profile management (software-defined, no truck roll for speed changes)
- Integration with OLT/ONU management system (CLI, SNMP, NETCONF)

**Billing Model:** **Postpaid (primary)** , **Prepaid (optional via credit control)**

**Typical Pricing:**
| Plan | Down | Up | Monthly Price (YER) | Monthly Price (USD) |
|---|---|---|---|---|
| Fiber 10 | 10 Mbps | 5 Mbps | 12,000 | $6 |
| Fiber 20 | 20 Mbps | 10 Mbps | 18,000 | $9 |
| Fiber 50 | 50 Mbps | 25 Mbps | 28,000 | $14 |
| Fiber 100 | 100 Mbps | 50 Mbps | 42,000 | $21 |
| Fiber 200 | 200 Mbps | 100 Mbps | 60,000 | $30 |
| Fiber 500 | 500 Mbps | 250 Mbps | 100,000 | $50 |

**Overdraft / Burstable:** Overages at YER 500/GB or speed throttling to 1 Mbps after FUP limit. FUP thresholds: 150 GB (10 Mbps), 300 GB (20+ Mbps).

**Promotions:** First month free on 12-month contract. Free ONT + installation on annual prepaid. Speed upgrade for 3 months on referral.

### 4.2 ADSL

**Definition:** DSL broadband over copper telephone lines. Legacy service for areas without fiber.

**Provisioning Requirements:**
- DSLAM port assignment
- VPI/VCI configuration
- PVC configuration (RFC 1483/2684 bridged or routed)
- PPPoE credentials generation
- Line profile (interleaved/fast, target SNR margin)
- Integration with DSLAM (CLI, SNMP)

**Billing Model:** **Prepaid (primary)** , **Postpaid (optional)**

**Typical Pricing:**
| Plan | Down | Up | Monthly Price (YER) |
|---|---|---|---|
| ADSL 4 | 4 Mbps | 512 Kbps | 8,000 |
| ADSL 8 | 8 Mbps | 1 Mbps | 12,000 |
| ADSL 16 | 16 Mbps | 2 Mbps | 18,000 |

**Notes:** ADSL is being phased out where fiber is available. No new ADSL activations in fiber-covered areas. FUP: 100 GB for 4 Mbps, unlimited for 8+ Mbps.

### 4.3 Fixed Wireless

**Definition:** Broadband delivered via fixed wireless (WiMAX, LTE FWA, or proprietary PtMP). Used in areas without fiber or DSL.

**Provisioning Requirements:**
- CPE registration (MAC, serial number)
- AP/BS sector assignment
- Subscriber station configuration (frequency, channel width, QoS profile)
- RSSI/SINR verification at installation
- Integration with wireless CPE management platform (SNMP, TR-069, or vendor API)

**Billing Model:** **Prepaid**

**Typical Pricing:**
| Plan | Down | Up | Monthly Price (YER) |
|---|---|---|---|
| Wireless 2 | 2 Mbps | 1 Mbps | 6,000 |
| Wireless 5 | 5 Mbps | 2 Mbps | 10,000 |
| Wireless 10 | 10 Mbps | 5 Mbps | 15,000 |
| Wireless 20 | 20 Mbps | 10 Mbps | 22,000 |

**Notes:** Capacity-constrained. Contention ratio 1:8. FUP: 50 GB for 2 Mbps, 100 GB for 5+ Mbps.

### 4.4 Yemen WiFi (Public WiFi Hotspots)

**Definition:** Public WiFi access points deployed in cafes, malls, universities, transportation hubs, public squares.

**Provisioning Requirements:**
- Hotspot controller integration (RADIUS, CoA, DM)
- Captive portal / splash page configuration
- Voucher code generation (PIN-based)
- Session management (time-based or data-based)
- MAC whitelisting / auto-login for subscribers
- Multi-SSID support (free tier vs. premium tier)

**Billing Model:** **Prepaid (voucher-based)** , **Complimentary (free tier)**

**Pricing:**
| Voucher | Data | Duration | Price (YER) |
|---|---|---|---|
| Day Pass | 500 MB | 24 hours | 500 |
| Weekly Pass | 3 GB | 7 days | 2,000 |
| Monthly Pass | 15 GB | 30 days | 5,000 |
| Free Tier | 100 MB | 1 hour (per day) | Free |

**Bundles:** FTTH/ADSL subscribers receive 5 GB/month free Yemen WiFi data as a loyalty perk.

### 4.5 Fixed Telephony (VoIP)

**Definition:** SIP-based voice service with geographic numbering (Yemen PSTN numbering plan +967). Includes national and international calling.

**Provisioning Requirements:**
- SIP trunk registration
- DID/MSISDN assignment from number pool
- SIP credentials generation (username, password, realm)
- Codec negotiation (G.711, G.729, G.722)
- CLIP/CLIR configuration
- Emergency call routing (direct to local emergency services if SIP trunk allows)
- Number portability database lookup

**Billing Model:** **Postpaid (subscription + usage)** or **Prepaid (credit-based)**

**Pricing:**
| Component | Price (YER) |
|---|---|
| Monthly line rental | 2,000 |
| National calls (per minute) | 30 |
| Mobile calls (per minute, on-net) | 40 |
| Mobile calls (per minute, off-net) | 60 |
| International (varies by zone) | 100–500 |
| Free on-net minutes (same ISP VoIP) | Included |

**Promotions:** Unlimited on-net calling (within ISP network) included with Fiber 100+ plans. 100 free national minutes per month on annual contracts.

### 4.6 Static IP

**Definition:** Publicly routable IPv4 address assigned to a subscriber's CPE. Single address or small block (/29, /28).

**Provisioning Requirements:**
- Public IP pool management
- IP-to-CPE binding (DHCP reservation or PPPoE fixed IP)
- Reverse DNS (rDNS/PTR record) upon request
- BGP announcement (if /24 or larger)

**Billing Model:** **Recurring add-on charge**

**Pricing:**
| Type | Monthly Price (YER) |
|---|---|
| Single Static IP | 3,000 |
| /29 block (5 usable) | 10,000 |
| /28 block (13 usable) | 25,000 |

**Rules:** Only available to postpaid accounts. Requires valid ID/CR documentation (regulatory requirement). Cannot be reassigned within billing period without prorated adjustment.

---

## 5. Business Services

### 5.1 DIA (Dedicated Internet Access)

**Definition:** Symmetric, uncontended internet access with SLA. Delivered via fiber, microwave, or satellite backhaul depending on location.

**Provisioning Requirements:**
- Physical handover (fiber tail, RJ45, SFP)
- L3 interface configuration (IP assignment, BGP peering or static routing)
- CIR/PIR configuration on PE router
- QoS policy marking (DSCP)
- SLA monitoring configuration (ICMP probes, NetFlow/jFlow/sFlow)
- Integration with router/switch management (CLI, NETCONF, RESTCONF)

**Billing Model:** **Postpaid (recurring + usage)**

**Pricing:**
| Bandwidth | CIR | Monthly Price (USD) |
|---|---|---|
| 10 Mbps | 10 Mbps | $200 |
| 50 Mbps | 50 Mbps | $600 |
| 100 Mbps | 100 Mbps | $1,000 |
| 500 Mbps | 500 Mbps | $3,500 |
| 1 Gbps | 1 Gbps | $5,000 |
| 10 Gbps | 10 Gbps | $15,000 |

**Usage Overage:** Burstable option: 95th percentile billing for usage above CIR at $15/Mbps/month.

**SLA Guarantees:**
- Uptime: 99.5% (standard), 99.9% (premium)
- Latency: <10 ms within Yemen, <150 ms international
- Jitter: <5 ms
- MTTR: <4 hours (standard), <2 hours (premium)
- SLA credits: 5% per hour of downtime, max 100%

### 5.2 Ethernet (Layer 2 Connectivity)

**Definition:** Point-to-point or point-to-multipoint Layer 2 Ethernet connectivity (E-LINE, E-LAN, E-TREE per MEF standards). Used for site-to-site connectivity, data center interconnect, and last-mile access for enterprise customers.

**Provisioning Requirements:**
- EVC (Ethernet Virtual Connection) provisioning
- VLAN or Q-in-Q configuration
- Bandwidth profile (CIR, EIR, CBS, EBS)
- CFM (Connectivity Fault Management) / Y.1731 OAM
- LAG/LACP configuration for multi-port handovers
- Integration with carrier Ethernet switches (CLI, NETCONF, RESTCONF)

**Billing Model:** **Postpaid (recurring)**

| Port Speed | Monthly Price (USD) |
|---|---|
| 100 Mbps | $150 |
| 1 Gbps | $400 |
| 10 Gbps | $1,500 |
| 100 Gbps | $8,000 |

**Notes:** Pricing is per port. Bandwidth profile and distance may adjust pricing. Available in Sana'a, Aden, and major economic zones.

### 5.3 PRI (Primary Rate Interface — VoIP Trunking)

**Definition:** ISDN PRI (E1, 30 channels) delivered over SIP trunking for enterprise PBX connectivity. Used for high-volume voice traffic.

**Provisioning Requirements:**
- SIP trunk provisioning on SBC (Session Border Controller)
- DID range allocation
- Concurrent call capacity configuration
- Codec negotiation (G.711 preferred for PRI compatibility)
- T.38 fax support
- Number presentation (CLIP/CLIR)
- Emergency call routing

**Billing Model:** **Postpaid (channel-based + usage)**

**Pricing:**
| Component | Price |
|---|---|
| Setup fee | $50 |
| Monthly per channel | $5 (30 channels = $150 E1) |
| National calls (per minute) | YER 20 |
| Mobile calls (per minute) | YER 35 |
| International (per minute) | $0.05–$0.50 |
| Unlimited on-net channels | Included |

### 5.4 VPS (Virtual Private Servers)

**Definition:** Virtualized server instances running on hypervisor infrastructure (KVM/VMware). Self-managed by customer.

**Provisioning Requirements:**
- Hypervisor pool management (CPU, RAM, storage)
- VM instantiation from template (OS images: Ubuntu, CentOS, Debian, Windows Server)
- Virtual network interface + public IP assignment
- VLAN/VXLAN isolation
- Storage allocation (local SSD, SAN/NFS)
- Backup schedule configuration
- KVM-over-IP / VNC console access
- API for provisioning automation (OpenStack or equivalent)

**Billing Model:** **Postpaid (prepaid options available)**

**Pricing:**
| Plan | vCPU | RAM | SSD | Bandwidth | Monthly Price (USD) |
|---|---|---|---|---|---|
| VPS-S | 1 | 1 GB | 25 GB | 1 TB | $10 |
| VPS-M | 2 | 4 GB | 50 GB | 2 TB | $25 |
| VPS-L | 4 | 8 GB | 100 GB | 4 TB | $50 |
| VPS-XL | 8 | 16 GB | 200 GB | 8 TB | $100 |
| VPS-XXL | 16 | 32 GB | 400 GB | 16 TB | $200 |

**Add-ons:** Extra IP ($3/mo), additional storage ($0.10/GB/mo), automated backup ($5/mo), DDoS protection ($10/mo).

### 5.5 Dedicated Servers

**Definition:** Physical bare-metal servers in data center. Full root access, no resource contention.

**Provisioning Requirements:**
- Rack/rack-unit assignment in DCIM
- Server PXE/iLO/iDRAC/IPMI configuration
- OS installation (automated PXE or manual ISO)
- Network switch port configuration (VLAN, port speed, LACP)
- Power circuit assignment (A+B feeds for redundancy)
- Cross-connect to customer patch panel
- Remote hands / smart hands support workflow

**Billing Model:** **Postpaid**

**Pricing:**
| Configuration | Monthly Price (USD) |
|---|---|
| E3 / 16 GB / 2x1TB HDD | $100 |
| E5 / 32 GB / 2x240GB SSD | $150 |
| Dual E5 / 64 GB / 4x480GB SSD | $300 |
| Dual Xeon Gold / 256 GB / 4x1TB NVMe | $600 |
| Custom configuration | Price on application |

**Contract terms:** Monthly, 6-month, 12-month (discounts: 5% for 6mo, 10% for 12mo).

### 5.6 Colocation

**Definition:** Physical space, power, and cooling for customer-owned equipment in carrier-grade data center.

**Provisioning Requirements:**
- Space reservation (cabinet, half-cabinet, quarter-cabinet, custom)
- Power circuit provisioning (A/B feeds, metered PDUs)
- Cross-connect provisioning (fiber, copper)
- Cage/g cabinet access control (badge, biometric)
- Remote hands request workflow
- Environmental monitoring (temperature, humidity, power)
- Integration with DCIM for capacity management

**Billing Model:** **Postpaid**

**Pricing:**
| Component | Monthly Price (USD) |
|---|---|
| Full cabinet (42U) | $500 |
| Half cabinet (21U) | $300 |
| Quarter cabinet (10U) | $200 |
| Space per U | $25 |
| Power per circuit (16A) | $150 |
| Redundant power (2nd 16A) | $100 |
| Cross-connect (fiber, single-mode) | $50 |
| Cross-connect (copper, Cat6) | $30 |
| Remote hands (per 30 min) | $25 |

**Notes:** Power billed at actual consumption via metered PDU. PUE adjustment factor applied.

### 5.7 Hosting (Shared Web Hosting)

**Definition:** Shared web hosting on Linux-based control panel (cPanel/WHM, DirectAdmin, or equivalent).

**Provisioning Requirements:**
- cPanel/WHM account creation
- DNS zone setup (authoritative nameservers)
- FTP/SSH credentials
- MySQL/MariaDB database creation
- Email account setup (POP3/IMAP/SMTP)
- Resource limit configuration (disk, inodes, CPU, RAM, processes, I/O)
- SSL certificate provisioning (Let's Encrypt auto)

**Billing Model:** **Prepaid (monthly, quarterly, semi-annual, annual)**

**Pricing:**
| Plan | Disk | Bandwidth | Domains | Monthly (YER) | Annual (YER) |
|---|---|---|---|---|---|
| Starter | 1 GB | 10 GB | 1 | 2,000 | 20,000 |
| Basic | 5 GB | 50 GB | 3 | 4,000 | 40,000 |
| Business | 20 GB | 200 GB | Unlimited | 8,000 | 80,000 |
| Reseller | 50 GB | 500 GB | Unlimited | 15,000 | 150,000 |

### 5.8 Domain Registration

**Definition:** Domain name registration and renewal. Supports gTLDs, ccTLDs (especially .ye, .com.ye, .org.ye, .net.ye for Yemen market).

**Provisioning Requirements:**
- Registry integration (EPP/REST API per TLD)
- WHOIS lookup
- DNSSEC management
- Auto-renewal configuration
- Domain push / transfer workflow
- .ye registrar accreditation compliance (YemenNet / TeleYemen)
- Contact validation (local presence required for .ye TLDs)

**Billing Model:** **Prepaid (annual recurring)**

**Pricing:**
| TLD | Registration (YER) | Renewal (YER) | Transfer (YER) |
|---|---|---|---|
| .com | 20,000 | 20,000 | 15,000 |
| .net | 22,000 | 22,000 | 17,000 |
| .org | 18,000 | 18,000 | 14,000 |
| .ye | 10,000 | 8,000 | N/A |
| .com.ye | 8,000 | 6,000 | N/A |
| .net.ye | 8,000 | 6,000 | N/A |
| .org.ye | 6,000 | 5,000 | N/A |

**Notes:** .ye TLDs require a local administrative contact and local presence in Yemen. Processing time for .ye domains is 1–3 business days (manual registry interaction may be required).

---

## 6. Hosting & Digital Services

*(Covered in Section 5 above: 5.4 VPS, 5.5 Dedicated Servers, 5.6 Colocation, 5.7 Shared Hosting, 5.8 Domain Registration.)*

### 6.1 Service Bundles

The platform must support packaged bundles combining multiple services with a single price:

| Bundle | Services Included | Price (USD/mo) |
|---|---|---|
| Business Starter | DIA 10 Mbps + Static IP + VPS-S + Domain (.com) | $250 |
| Business Professional | DIA 50 Mbps + /28 IP block + VPS-L + Domain + Hosting Basic | $700 |
| Enterprise Suite | DIA 100 Mbps + Ethernet 1 Gbps (2 sites) + PRI (1 E1) + Colo (half cabinet) | $1,800 |
| Developer Stack | VPS-XL + Dedicated Server (E5) + /28 IP block | $350 |

---

## 7. Revenue Requirements

### 7.1 Revenue Recognition

| Charge Type | Recognition Method | Timing |
|---|---|---|
| One-time fees (setup, installation, activation) | Recognized at point of service activation | Upon successful provisioning |
| Monthly recurring charges | Straight-line over service period | First day of service period |
| Usage charges (voice minutes, data overage) | Accrual basis at time of usage event | Daily batch, recognized on usage date |
| Prepaid vouchers (Yemen WiFi) | Deferred revenue, recognized as consumed | Upon voucher usage |
| Annual prepaid subscriptions | Deferred revenue, recognized monthly over contract term | Pro-rata monthly |
| SLA credits | Reduction of revenue in period incurred | At time of SLA breach verification |

### 7.2 Invoicing Cycles

| Segment | Cycle Type | Invoice Generation | Due Date | Late Payment |
|---|---|---|---|---|
| Residential (postpaid) | Monthly, calendar month | 1st of month | 15th of month | Suspension on 20th |
| SMB (postpaid) | Monthly, calendar month | 1st of month | 15th of month | Suspension on 25th |
| Enterprise | Monthly or quarterly, anniversary-based | Last day of billing period | Net-30 from invoice date | Suspension on Net-40 |
| Wholesale | Monthly, calendar month | 5th of month | Net-30 | Suspension on Net-45 |
| Prepaid customers | No invoice (voucher/credit-based) | N/A | N/A | N/A |
| Hosting/Domains | Annual or per-term renewal invoice | 30 days before expiry | 7 days before expiry | Suspension on expiry |

### 7.3 Tax Handling — Yemen Market

**Regulatory Context:**
- Republic of Yemen does not operate a general VAT system
- The applicable tax on services is **Withholding Tax (WHT)** at **5%** on the total invoice amount for business-to-business transactions
- Residential consumers are generally not subject to withholding tax
- Tax invoices must include: Taxpayer ID (TIN) of both parties, invoice date, description of services, amount before tax, WHT amount, net payable amount

**Tax Configuration:**
| Scenario | Tax Rule |
|---|---|
| Residential customer | No tax applied |
| Business customer with valid TIN | 5% withholding tax applied to invoice total |
| Business customer without TIN | 5% withholding tax applied (higher rate may apply per tax authority) |
| Prepaid services | Tax applied at point of purchase (included in price) |
| Cross-border services (inbound) | Subject to 5% WHT where applicable |
| Export of services (outbound) | Zero-rated if outside Yemen |

**Invoice Tax Display:**
```
Description of Service
Subtotal:              YER 100,000
Withholding Tax (5%):   YER   5,000
Total Due:              YER  95,000
```

**Note:** The net payable is subtotal minus WHT, as the customer withholds the tax amount and remits it directly to the Yemen Tax Authority (YTA). The invoice must clearly state: "The withholding tax amount must be deducted and remitted to the Yemen Tax Authority."

### 7.4 Discounts & Promotions

| Discount Type | Rules | Compounding |
|---|---|---|
| Volume discount | Applied to total monthly charges > threshold | Stackable with one other discount type |
| Contract term discount | 5% (6mo), 10% (12mo), 15% (24mo) | Applies before promotions, after volume discount |
| Promotional discount | Fixed % or amount, limited duration (max 6 months) | Applied last |
| Referral credit | One-time credit of YER 5,000 per successful referral | Applies as account credit, not discount |
| Bundle discount | Pre-defined bundle price | Not stackable with other discounts |
| Loyalty discount | 5% after 12 months, 10% after 24 months continuous service | Stackable with volume discount only |
| Early payment discount | 2% if paid within 7 days of invoice date | Exclusive, cannot combine with other discounts |

**Discount Compounding Rules:**

1. **Order of application:** Volume → Contract Term → Bundle → Promotional → Loyalty
2. **Stacking limit:** Maximum 2 discount types can be active on a single subscription
3. **Cap:** Total discount cannot exceed 50% of the base recurring charge
4. **Proration:** Discounts are prorated when applied or removed mid-cycle
5. **Promotional discounts cannot be combined** with other promotional discounts
6. **Referral credits** are non-monetary account credits, not discounts, and are not limited by the stacking rules

**Example — Discount Calculation:**
```
Base Monthly Charge:          YER 60,000 (Fiber 200)
Volume Discount (10%):       -YER  6,000
Contract Term Discount (5%):  -YER  2,700 (applied after volume discount: 54,000 × 5%)
Promotional Discount (10%):  -YER  5,130 (applied after contract term: 51,300 × 10%)
Loyalty Discount (5%):       -YER  2,308 (applied next: 46,170 × 5%)
Total After Discounts:        YER 43,862
Total Discount:               26.9% (within 50% cap)
```

### 7.5 Loyalty Program

| Tier | Qualification | Benefits |
|---|---|---|
| Bronze | 6 months continuous | 5% discount on Yemen WiFi vouchers |
| Silver | 12 months continuous + spend > YER 15,000/mo | Bronze + 5% monthly discount, priority support |
| Gold | 24 months continuous + spend > YER 30,000/mo | Silver + 10% discount, free static IP, dedicated support line |
| Platinum | 36 months continuous + spend > YER 60,000/mo | Gold + 15% discount, free installation, VIP support |

**Loyalty Rules:**
- Continuous service required; suspension > 7 days resets tier clock
- Spend is average of last 3 months' charges (excluding one-time fees)
- Tiers are evaluated monthly on billing date
- Downgrade: 2 consecutive months below spend threshold triggers next-month downgrade

---

## 8. Billing Requirements

### 8.1 Rating Engine

The rating engine must support:

| Capability | Description |
|---|---|
| Usage rating | Real-time and batch rating of data (GB/MB), voice (minutes), SMS (messages) |
| Event-based rating | Rating per usage event with configurable unit pricing |
| Tiered rating | Different rates at different consumption levels (e.g., first 100 GB at rate A, excess at rate B) |
| Volume-based rating | Rating based on total volume consumed |
| Time-based rating | Different rates by time of day (peak/off-peak), day of week, calendar dates |
| Zone rating | Different rates by destination (national, mobile, international zones) |
| Prepaid balance deduction | Real-time balance check and deduction before service delivery |
| Credit control | Threshold-based credit management for postpaid (soft limit, hard limit) |
| Session rating | Rating per session with minimum charge and rounding increments |
| Recursive rating | Rating that considers cumulative usage across multiple services |

**Rounding Rules:**
- Voice: Per-second granularity, minimum 1 second
- Data: Per-kilobyte granularity, minimum 1 KB
- Monetary amounts: To nearest YER (no sub-unit), to nearest cent (USD)

### 8.2 Usage Collection

| Collection Point | Technology | Data Collected | Frequency |
|---|---|---|---|
| BNG/BRAS | RADIUS accounting, CoA | Session start/stop, interim updates, octets, session ID, NAS-IP, called/calling station ID | Real-time (start/stop), 15-min interim |
| OLT | SNMP, NETCONF | Bytes per ONT per VLAN, optical power levels | 15 minutes |
| CPE | TR-069/TR-181 | Bytes sent/received, connection time, signal metrics | 30 minutes |
| SBC | SIP CDR, RADIUS | Call start/end, duration, calling/called party, termination cause, codec | Real-time |
| Firewall | NetFlow v9/IPFIX, sFlow | Flow records: src/dst IP, ports, protocol, bytes, packets, timestamps | 5 minutes |
| Hypervisor | libvirt API / vCenter API | VM CPU, RAM, disk I/O, network bytes | 5 minutes |
| PDU/DCIM | SNMP, Modbus | Power consumption per circuit/outlet, temperature, humidity | 5 minutes |

**Usage Record Retention:**
- Real-time (detailed) CDRs: 90 days online, 7 years archived
- Billing records (aggregated): 10 years
- RADIUS accounting logs: 12 months
- Flow records: 30 days

### 8.3 Recurring Charges

| Charge Type | Application | Proration |
|---|---|---|
| Monthly subscription | All recurring services | Daily proration (30-day divisor) |
| IP address rental | Static IP, IP blocks | Daily proration |
| Domain renewal | Annual | No proration (annual charge at renewal) |
| Power circuits (colo) | Metered — monthly | Daily proration |
| Cross-connects | Monthly | Daily proration |
| Maintenance/support | Monthly or annual | Daily proration |
| SLA premium | Monthly | Daily proration |

**Proration Formula:**
```
Prorated Amount = (Recurring Monthly Amount / 30) × Days of Service
```

**Contract-related charges:**
- Early termination fee: 50% of remaining contract value
- Service downgrade within contract term: Not permitted; upgrade permitted with contract extension
- Suspension fee: YER 2,500 (applied on each suspension event)

### 8.4 One-Time Charges

| Charge Type | Application |
|---|---|
| Installation fee | FTTH/ADSL/Fixed Wireless — YER 5,000–25,000 depending on service |
| Activation fee | Service activation — YER 2,000 |
| Shipping fee | Equipment delivery — YER 3,000 |
| Equipment fee | CPE/ONT/CPE modem — YER 10,000–50,000 (purchase or installment) |
| Change fee | Service modification — YER 2,000 |
| Relocation fee | Move service to new address — YER 10,000 |
| Reconnection fee | Reconnect after suspension — YER 5,000 |
| Late payment fee | See Section 13.2 |
| Check/cheque return fee | Insufficient funds — YER 10,000 |
| Administrative fee | Special requests — Price on application |

### 8.5 Billing Cycles

| Cycle | Applicable To | Description |
|---|---|---|
| Calendar Monthly | Residential, SMB, Wholesale | Billing period: 1st to last day of month Invoice: 1st of following month |
| Anniversary Monthly | Enterprise | Billing period: service activation date to preceding date next month |
| Quarterly | Enterprise (optional) | 3-month billing period |
| Annual | Hosting, Domain, VPS | 12-month billing period, billed in advance |
| Prepaid | Fixed wireless, Yemen WiFi | Credit purchased in advance, consumed until exhausted |
| Weekly | Yemen WiFi vouchers | Time-limited vouchers |

### 8.6 Billing Schedules

| Event | Timing | Action |
|---|---|---|
| Cycle open | Start of billing period | Begin accumulating charges |
| Cycle close | End of billing period | Stop accumulating, generate rating batch |
| Invoice generation | Day 1 of next cycle (or anniversary) | Generate PDF invoice, post to customer portal |
| Invoice delivery | Day 1 | Email, SMS notification, portal notification, printed copy (upon request) |
| Payment due | Day 15 (residential), Net-30 (enterprise) | Payment expected |
| Grace period | Due date + 5 calendar days | Late payment fee applies after grace period |
| First reminder | Day 17 (residential), Net-30 + 3 (enterprise) | Automated SMS + email |
| Second reminder | Day 19 | Automated call + SMS |
| Final notice | Day 20 | Warning of suspension, SMS + email + call |
| Suspension | Day 21 | Service suspended per Section 9.2 |
| Termination | 30 days after suspension | Service terminated per Section 9.4 |

### 8.7 Dunning / Collections

**Dunning Workflow:**

```
Stage 0: Invoice Issued
    └── Normal state

Stage 1: Due Date Passed (Day 16)
    ├── No action if partial payment received
    └── Send gentle reminder (SMS, email)

Stage 2: Grace Period (Day 16–20)
    ├── Late payment fee applied (Day 16)
    ├── Daily automated reminder
    └── Customer can pay without any restriction

Stage 3: Soft Suspension (Day 21–30)
    ├── Outbound calls (call center)
    ├── SMS hourly on Day 21 
    ├── Service rate-limited to 256 Kbps
    ├── Outbound calls blocked (incoming allowed)
    └── Partial payment accepted (50% minimum to restore)

Stage 4: Hard Suspension (Day 31–60)
    ├── All services blocked
    ├── Automated calls (IVR) daily
    ├── Email and SMS every 3 days
    └── Multiple services: all or none suspension (unless partial payment)

Stage 5: Collections (Day 61–90)
    ├── Account transferred to collections team
    ├── Manual outreach
    ├── Physical notice delivery possible
    ├── Account flagged for legal escalation
    └── Early termination fee assessment considered

Stage 6: Termination (Day 91+)
    ├── All services terminated permanently
    ├── Number returned to pool (DID, IP)
    ├── Equipment repossession order (if leased)
    ├── Final invoice including outstanding balance + termination fees
    ├── Account sent to external collections / legal
    └── Customer record blacklisted in CRM
```

**Collection Rules:**
- Multiple services on one account: suspension applies to all services unless partial payment is made for specific services
- Collection agency fee: 15% of recovered amount added to customer balance
- Payment plan option available at Stage 3: minimum 30% upfront, remainder over 3 months with 2% monthly interest
- Government / embassy accounts: Special escalation path (diplomatic channel, procurement office)
- Humanitarian / NGO accounts: 30-day extension available upon request

---

## 9. Provisioning Requirements

### 9.1 Activation Workflow

**FTTH Activation:**
```
Order Approval → Inventory Check (OLT port, splitter port, fiber availability)
    → Resource Reservation → Technician Dispatch (Field Service)
        → Fiber Installation & ONT Mounting → ONT Registration (OLT)
            → Service Profile Assignment (Bandwidth, VLAN, QoS)
                → PPPoE Credentials / DHCP Reservation
                    → First Mile Testing (RSSI, throughput, latency)
                        → Customer Acceptance → Service Activation → Billing Start
```

**ADSL/Fixed Wireless Activation:**
```
Order Approval → Inventory Check (DSLAM port / AP sector capacity)
    → Resource Reservation → Technician Dispatch
        → CPE Configuration → Line Provisioning
            → First Mile Testing → Customer Acceptance
                → Service Activation → Billing Start
```

**DIA Activation:**
```
Order Approval → SLA Validation (capacity exists on upstream)
    → Network Design Review → L3 Handover Configuration
        → IP Assignment → BGP Peering / Static Route Setup
            → QoS Policy → Monitoring Setup (SNMP, NetFlow, ICMP probe)
                → Customer Handover → Service Activation → Billing Start
```

**VPS Activation:**
```
Order Approval → Resource Allocation Check (Hypervisor capacity)
    → VM Instantiation (Template-based) → IP Assignment
        → DNS Config → Credential Generation (root/admin password, SSH key)
            → Monitoring Setup → Customer Notification → Service Activation → Billing Start
```

### 9.2 Suspension Workflow

**Soft Suspension (Dunning Stage 3):**
```
Trigger: Dunning Stage 3 reached (Day 21)
    → Update Customer Status to "Suspended (Soft)"
        → Apply QoS rate-limit policy (256 Kbps)
            → Block outbound voice calls (SBC routing update)
                → Block VPS / Dedicated Server (shutdown VM, IPMI power-off)
                    → Update billing state: continue accruing recurring charges
                        → Send notification to customer
```

**Hard Suspension (Dunning Stage 4):**
```
Trigger: Dunning Stage 4 reached (Day 31)
    → Update Customer Status to "Suspended (Hard)"
        → Remove RADIUS session / terminate PPPoE session
            → Disable ONT (deactivate GEM port)
                → Block all SIP registration
                    → Power-off VMs / Dedicated Servers
                        → Update billing state: suspend recurring charges (no accrual during hard suspension)
                            → Send final notification
```

**Administrative Suspension (policy violation, security issue):**
```
Manual Trigger (CSR or Admin)
    → Immediate hard suspension
        → Case created in ticketing system
            → Legal review within 24 hours
                → Notification to customer with reason
```

**Resumption from Suspension:**
```
Payment Received → Clear outstanding balance (or payment plan agreement)
    → Update billing state → Trigger provisioning resume
        → Remove rate-limit policy → Enable ONT/RADIUS/SIP/VM
            → Customer Status → "Active"
                → Send confirmation notification
```

### 9.3 Deactivation / Termination Workflow

**Voluntary Termination (customer request):**
```
Customer Request → CSR verification (identity, contract obligations)
    → Early Termination Fee calculation (if applicable)
        → Final invoice generation → Payment of final balance
            → Deactivate services → Return DIDs/IPs to pool
                → Equipment return coordination (if leased)
                    → Customer Status → "Terminated"
                        → Exit survey → Post-termination retention offer
```

**Involuntary Termination (non-payment Stage 6):**
```
Dunning → Day 91 → Update Status → "Terminated"
    → Deactivate all services
        → Return numbers/IPs to pool
            → Equipment repossession order
                → Final invoice → External collections
                    → Blacklist customer record
```

**Deactivation Technical Steps:**
- FTTH: Delete ONT from OLT (or disable GEM port), remove PPPoE profile, release IP
- ADSL: Remove DSL profile from DSLAM, release PVC, disable PPPoE
- Fixed Wireless: Remove CPE from base station, deactivate RADIUS account
- DIA: Remove BGP session, deconfigure interface, return IP block to pool
- VPS: Destroy VM, release IP, delete storage volumes (or archive per policy)
- Dedicated Server: Format drives, power off, return to inventory, wipe BMC config
- Colocation: Cancel cross-connects, schedule equipment removal, revoke access
- PRI/SIP: Remove SIP trunk from SBC, return DID range, update routing

**Retention Policy Before Deactivation:**
- 7-day data retention after termination (backup available upon request)
- Customer data archived for 3 years (legal requirement)
- Billing records retained for 10 years

### 9.4 Service States

```
Active
  ├── Active Normal (fully operational)
  ├── Active Pending (awaiting installation/activation)
  └── Active Suspended
        ├── Soft Suspended (rate-limited)
        └── Hard Suspended (fully blocked)

Pending Activation → Active → Suspended → Active → Suspended → Terminated
                                         (cycle can repeat)
```

### 9.5 Network Element Integration

| Element | Protocol | Provisioning Actions | Vendor Examples |
|---|---|---|---|
| OLT | NETCONF, CLI (SSH), SNMP | Create/delete ONT, assign bandwidth profile, config VLAN, QoS | Huawei (MA5800, MA5600T), ZTE (C300, C320), Nokia (ISAM, 7360) |
| BNG/BRAS | RADIUS (CoA), CLI (SSH) | PPPoE service profile, IP pool assignment, QoS policy | Huawei (ME60, NE40), Cisco (ASR9K, ASR1K), Juniper (MX) |
| DSLAM | CLI (SSH), SNMP | VPI/VCI config, PVC, line profile | Huawei (MA5600, MA5603), ZTE (9800 series), Ericsson (D50) |
| Wireless AP/BS | CLI (SSH), SNMP, HTTP API | CPE registration, QoS profile, sector assignment | Cambium (PMP, ePMP), Ubiquiti (airMAX), Mimosa |
| SBC | REST API, SOAP, CLI | SIP trunk, DID range, routing rules | Audiocodes (Mediant), Oracle (Acme Packet), Ribbon (SBC) |
| Router/Switch | CLI (SSH), NETCONF, RESTCONF | L3 interface, BGP policy, QoS, ACL | Cisco (IOS-XR, IOS-XE), Juniper (Junos), Huawei (VRP), Arista (EOS) |
| Hypervisor | REST API, libvirt | VM create/destroy, network config, storage | KVM/libvirt, VMware vSphere, Proxmox, Xen |
| DCIM | SNMP, REST API | Power circuit, rack unit, PDU outlet | Sunbird (dcTrack), Nlyte, Device42 |
| cPanel | WHM API, REST API | Account create/delete, DNS zone, resource limits | cPanel/WHM, DirectAdmin |
| Domain Registry | EPP, REST API | Domain register/renew/transfer | Nominet (for .uk), CentralNic, Verisign (for .com/.net) |
| Hotspot Controller | RADIUS, REST API | Voucher create/activate/deactivate, session control | MikroTik (RouterOS), Cambium (cnMaestro), PacketFence |

---

## 10. Customer Lifecycle

### 10.1 Lifecycle States

```
                    ┌──────────────────────────────────────────────┐
                    │                 Lead                         │
                    │  (Not yet contacted, no service interest)    │
                    └────────────────────┬─────────────────────────┘
                                         │ Qualification
                    ┌────────────────────▼─────────────────────────┐
                    │               Prospect                        │
                    │  (Interested, in sales pipeline)             │
                    └────────────────────┬─────────────────────────┘
                                         │ Order Placed
                    ┌────────────────────▼─────────────────────────┐
                    │               Customer                        │
                    │  (Order placed, may not yet be active)       │
                    └────────────────────┬─────────────────────────┘
                                         │ Service Activated
                    ┌────────────────────▼─────────────────────────┐
                    │               Active                          │
                    │  (At least one service in Active state)      │
                    └───────┬────────────────────┬─────────────────┘
                            │                    │
                   Suspension Event     Voluntary Churn
                            │                    │
                    ┌───────▼────────┐  ┌────────▼─────────┐
                    │   Suspended    │  │   Terminated      │
                    │ (Soft or Hard) │  │ (Voluntary)       │
                    └───────┬────────┘  └────────┬─────────┘
                            │                    │
                    Payment Rece'd        Involuntary
                            │                    │
                    ┌───────▼────────┐  ┌────────▼─────────┐
                    │  Active (Rsm)  │  │   Terminated      │
                    └────────────────┘  │ (Involuntary)     │
                                        └──────────────────┘
```

### 10.2 State Transitions

| From | To | Trigger | Conditions | System Actions |
|---|---|---|---|---|
| Lead | Prospect | Lead qualification | Lead score > threshold, or sales rep assignment | Create opportunity, assign to sales queue |
| Prospect | Customer | Order placed | Quote accepted, order created | Create account, create subscription |
| Customer | Active | Service provisioned | Order Fulfillment completed, provisioning confirmed | Begin billing, send welcome notification |
| Active | Suspended (Soft) | Dunning Stage 3 | Invoice overdue > 5 days past grace period | Rate-limit service, notify customer |
| Active | Suspended (Hard) | Dunning Stage 4 | Overdue > 15 days past grace period | Block all services, stop recurring charge accrual |
| Active | Active (change) | Service modification | Service change order approved | Provisioning workflow, prorated billing adjustment |
| Active | Terminated (Voluntary) | Customer request | Verification successful, ETF paid | Deactivate services, final invoice, exit process |
| Suspended | Terminated (Involuntary) | Dunning Stage 6 | Overdue > 90 days | Deactivate services, final invoice, collections |
| Suspended | Active | Payment or arrangement | Balance cleared or payment plan agreed | Restore services, resume billing |
| Terminated | Lead/Prospect | Retention offer accepted | Customer re-engages within 90 days | New lead record (not restored), simplified onboarding |

### 10.3 Customer Status Definitions

| Status | Definition |
|---|---|
| Lead | Contact in CRM, no expression of interest. Source: purchased list, event, website visit. |
| Prospect | Qualified lead with demonstrated interest. Has at least one interaction (call, visit, email response). |
| Customer | Has a signed order (or digital acceptance). At least one service in Pending Activation or Active state. |
| Active | At least one service in Active state. Billing active. |
| Suspended (Soft) | Service rate-limited. Voice outbound blocked. VMs/VMware still powered on. |
| Suspended (Hard) | All services blocked. No charges accruing. Account still open. |
| Terminated (Voluntary) | All services deactivated at customer's request. Account closed. |
| Terminated (Involuntary) | All services deactivated due to non-payment or policy violation. Account flagged. Blacklisted. |

---

## 11. Order Lifecycle

### 11.1 Lifecycle Stages

```
Cart → Quote → Order → Order Items → Fulfillment → Activation → Billing Start
```

### 11.2 Detailed Flow

**Stage 1: Cart**
```
Customer browses service catalog (web portal, CSR-assisted, walk-in)
    → Select services, quantities, term length, add-ons
        → Preliminary eligibility check (address coverage, credit check for enterprise)
            → Cart total displayed (including one-time fees, recurring charges estimated)
                → Save as draft or proceed to quote
```

**Stage 2: Quote**
```
Cart → Generate Quote
    → Quote number assigned
        → Price locked for 14 days (residential) / 30 days (business)
            → Quote PDF generated, sent to customer
                → Customer accepts via: e-signature, SMS confirmation, verbal (call recording), physical signature
```

**Stage 3: Order**
```
Quote Accepted → Create Order
    → Order number (auto-generated, format: ORD-{YYYY}-{XXXXX})
        → Customer account created (if new) or verified (if existing)
            → Required documentation collected:
                - Residential: National ID (Sareeha) or Passport
                - Business: Commercial Registration (CR), Taxpayer ID (TIN)
                - Enterprise: CR, TIN, board resolution / authorized signatory letter
            → Credit verification (enterprise only): credit limit check
                → Order status → "Pending Approval"
                    → Automated approval (rules-based) or manual approval (high-value / flagged orders)
                        → Order status → "Approved"
```

**Stage 4: Order Items**
```
Order → Split into Order Items (one per service)
    → Each Order Item has:
        - Service type (FTTH, DIA, VPS, etc.)
        - Offer/plan selected
        - Quantity
        - One-time charges
        - Recurring charges
        - Term (MBC or no-commitment)
        - Scheduled activation date
        - Delivery address (for physical services)
```

**Stage 5: Fulfillment**
```
Each Order Item → Fulfillment Plan
    → Dependent tasks grouped into fulfillment flows
        Examples:
        - FTTH: "Check OLT Port Availability" → "Assign OLT/Splitter" → "Create Work Order for Installer" → "Install ONT" → "Register ONT" → "Test & Accept"
        - VPS: "Check Hypervisor Capacity" → "Select Host" → "Create VM" → "Assign IP" → "Configure DNS"
        - Domain: "Check Availability" → "Register via EPP" → "Configure DNS" → "Setup Auto-Renewal"
    → Task assignment (auto-assign to field techs, network ops, or system-provisioned)
    → Tracking: individual task status, ETA, dependencies
    → Escalation: if fulfillment exceeds SLA, escalate to fulfillment manager
```

**Stage 6: Activation**
```
Fulfillment Complete → Verify activation (automated health check)
    → Update subscription state to Active
        → Send activation notification (SMS, email, portal notification)
            → Include service credentials (CPE login, PPPoE username/password, IP info, DNS servers)
```

**Stage 7: Billing Start**
```
Activation confirmed → Trigger billing start schedule
    → Set billing_start_date = activation date
        → Generate first bill: prorated from activation date to end of current cycle
            → First invoice: includes prorated charges + any one-time setup fees
                → Recurring billing begins on next cycle date
```

### 11.3 Order Statuses

| Status | Description |
|---|---|
| Draft | Cart saved, not yet converted to order |
| Quoted | Quote issued, awaiting customer acceptance |
| Pending Approval | Order submitted, awaiting internal approval |
| Approved | Order approved, moving to fulfillment |
| In Fulfillment | At least one order item in fulfillment |
| Partially Active | Some services activated, some pending |
| Completed | All order items activated and billing started |
| Cancelled | Order cancelled before full activation |
| Disputed | Customer disputing the order/charges |
| On Hold | Order paused for manual intervention (e.g., missing docs, credit hold) |

### 11.4 Order Modification

**Pre-Fulfillment Modification:**
- Customer can modify order before fulfillment begins
- Full re-quote required; price lock may reset if modification extends 14-day quote period
- No fees for pre-fulfillment modifications

**Post-Activation Modification (Change of Service):**
- Change Order created (separate from original order)
- Prorated charges/credits for remaining period
- Upgrade: immediate activation, contract extension (6 months minimum)
- Downgrade: effective next billing cycle, no contract extension
- Change fee: YER 2,000

---

## 12. Payment Methods

### 12.1 Supported Payment Methods

| Method | Segment | Currency | Real-time | Notes |
|---|---|---|---|---|
| Cash | Residential, SMB | YER | Yes | Walk-in payment at ISP branches or agent network |
| Bank Transfer (local) | SMB, Enterprise, Wholesale | YER, USD | No (1-2 days) | Requires manual reconciliation or bank integration |
| Mobile Money | Residential | YER | Yes | MTN Mobile Money, Yemen Mobile (Sahalat), Y-Cash |
| Credit/Debit Card | SMB, Enterprise, Wholesale | USD | Yes | Visa/Mastercard via payment gateway; limited Yemen-issued cards |
| Payment Gateway (online) | All | USD | Yes | Stripe, PayTabs (MENA-focused), Telr |
| Cheque / Check | Enterprise, Wholesale | YER, USD | No (3-5 days clearing) | Acceptance subject to credit check; returned check fee applies |
| Direct Debit | Enterprise (stable accounts) | YER, USD | Batch (monthly) | Requires signed mandate; limited bank support in Yemen |
| Prepaid Voucher | Residential, SMB | YER | Yes | Scratch card PIN: purchased at agents, entered in portal |
| Wallet (platform balance) | All | YER, USD | Yes | Internal wallet; topped up via any method; used for auto-pay |
| USD Cash | Enterprise, Wholesale | USD | Yes | For USD-denominated invoices; accepted at select branches |

### 12.2 Yemen-Specific Payment Context

**Banking Infrastructure Constraints:**
- Many Yemeni banks operate on SWIFT but domestic interbank transfers can be slow (1–3 days)
- Sana'a and Aden have separate banking networks due to the political situation
- Multiple exchange rates exist (official, parallel market, and remittance rates)
- The platform must support payment collection and reconciliation at both official and agreed-upon exchange rates

**Mobile Money Dominance:**
- Mobile money (MTN Mobile Money, Yemen Mobile Sahalat) is widely used even in rural areas
- Mobile agent network (hawalas/remittance shops) provides cash in/out points
- Real-time payment confirmation via USSD or mobile app
- Integration required: MTN Mobile Money API, Yemen Mobile Sahalat API

**Agent Network:**
- For cash collection in areas without bank branches
- Agents are retail shops, Amana (remittance) offices, mobile money agents
- Agent settlement: collected cash → agent receives commission (1–3%)
- Agent management module required: onboarding, float management, reconciliation, commission calculation

**Exchange Rate Handling:**
```
USD Invoice → Customer pays in YER
    → Exchange rate = [Agreed rate or parallel market rate]
        → Invoice paid in YER equivalent
            → System records: USD amount, YER amount, exchange rate used, rate source
```

| Rate Source | Definition | Used For |
|---|---|---|
| Official Rate | Central Bank of Yemen rate | Government accounts, official transactions |
| Parallel Market Rate | Market-driven rate (higher than official) | Most commercial transactions |
| Agreed Contract Rate | Rate specified in customer contract | Enterprise accounts with long-term contracts |
| Platform Rate | Rate configured in system, updated X times daily | Default for auto-conversion |

### 12.3 Payment Reconciliation

| Source | Data | Reconciliation Method |
|---|---|---|
| Bank Transfer (MT103) | Remitter name, amount, date, reference | Manual match by AR team or automated via bank API |
| Mobile Money | Transaction ID, MSISDN, amount, timestamp | Real-time callback API → auto-reconciliation |
| Card Gateway | Transaction ID, PAN (masked), amount, auth code | Real-time callback + batch settlement file |
| Cash (branch) | Payment receipt ID, cashier ID, amount | Daily POS cash-up report |
| Agent Payment | Agent ID, customer number, amount | Agent disbursement batch file |
| Wallet | Internal ledger | Automatic on transaction |

**Reconciliation SLA:**
- Auto-reconciled payments: < 1 minute
- Bank transfers: < 24 hours (manual), < 1 hour (automated via bank API)
- Cheques: at clearing (3–5 days)
- End-of-day reconciliation: daily by 2:00 AM

### 12.4 Payment Allocation Rules

| Rule | Description |
|---|---|
| FIFO Allocation | Payments applied to oldest outstanding charges first |
| Service-Locked Allocation | Customer can specify which service the payment is for |
| Partial Payment | Applied proportionally across all outstanding items |
| Discount Application | Early payment discount applied before allocation |
| Prepayment / Credit | Payments received before invoice due date apply as account credit |

---

## 13. Business Rules

### 13.1 Dual-Currency Support (YER/USD)

| Rule | Description |
|---|---|
| Billing currency | Determined by customer segment and service: Residential → YER; SMB → YER or USD; Enterprise → USD; Wholesale → USD |
| Invoice currency | Must match the billing currency of the subscription |
| Multi-currency account | A single account can have YER and USD subscriptions (separate invoices) |
| Payment in non-invoice currency | Customer may pay in opposite currency at the platform exchange rate |
| Exchange rate update frequency | Minimum 1x daily; configurable per source (official vs. parallel) |
| Reporting | All financial reports must show amounts in both YER (primary) and USD (secondary) with the exchange rate at report generation time |
| Tax calculation | Withholding tax calculated in invoice currency |
| Rounding | YER to nearest whole number; USD to nearest cent (0.01) |
| Minimum payment | YER: 500; USD: $1 |
| Balance display | Customer portal shows balances in both YER and USD |

### 13.2 Late Payment Fees

| Segment | Late Fee | Applies After |
|---|---|---|
| Residential | YER 2,000 flat fee | 5 days past due date |
| SMB | YER 5,000 flat fee | 5 days past due date |
| Enterprise | 2% of overdue amount (min $50, max $200) | 5 days past due date |
| Wholesale | 2.5% of overdue amount (min $100, max $500) | 5 days past due date |
| Hosting | Service suspension (no late fee) | 7 days before expiry |

**Late Fee Rules:**
- Applied once per billing cycle, not per day
- Not charged if total overdue amount < YER 2,000 / $5
- Cap: late fees cannot exceed 100% of the original invoice amount
- Waivable by CSR with supervisor approval (max 1 waiver per customer per 12 months)

### 13.3 Service Suspension Policy

**Grace Periods:**
| Segment | Grace Period (from due date) | Suspension Trigger |
|---|---|---|
| Residential | 5 days | Day 21 (soft), Day 31 (hard) |
| SMB | 5 days | Day 25 (soft), Day 35 (hard) |
| Enterprise | 30 days (Net-30 terms) | Day 40 (soft), Day 50 (hard) |
| Wholesale | 30 days (Net-30 terms) | Day 45 (soft), Day 60 (hard) |

**Suspension Thresholds (for partial payment):**
- Soft suspension threshold: 50% of total overdue must be paid to avoid
- Hard suspension threshold: 75% of total overdue must be paid to avoid
- Resumption threshold: 100% of total overdue (or payment plan agreement)

**Exceptions (no suspension even if overdue):**
- Government accounts with PO/invoice in process
- Accounts under payment plan agreement (with current payments)
- Accounts flagged by legal or executive team (limited to 30 days, requiring renewal)
- Humanitarian/UN/NGO accounts with valid funding letter

### 13.4 Collection Escalation

See [Section 8.7](#87-dunning--collections) for full dunning workflow.

**Escalation Matrix:**

| Amount (USD) | Days Overdue | Action | Responsible |
|---|---|---|---|
| < $50 | 1–20 | Automated reminders | System |
| $50–$500 | 21–60 | Call center + soft/hard suspension | Collections Team |
| $500–$5,000 | 61–90 | Collections team + legal warning | Senior Collections |
| $5,000+ | 61–90 | Executive involvement + legal escalation | Collections Manager + Legal |
| Any | 90+ | Termination + external collections + legal | Legal |

**Payment Plan Rules:**
- Available at Stage 3 (Soft Suspension) or Stage 4 (Hard Suspension)
- Minimum 30% upfront payment required
- Maximum 3-month term
- 2% monthly interest on outstanding balance
- Payment plan defaults: immediate hard suspension + forfeiture of all discounts applied during plan
- Maximum one payment plan per customer per rolling 12-month period

### 13.5 Tax Handling

**Tax Configuration:**
| Parameter | Value |
|---|---|
| Tax regime | Withholding Tax (WHT) |
| Rate | 5% of invoice total |
| Applicable to | B2B transactions (customer has valid TIN) |
| Residential exemption | Yes — no tax on residential services |
| Exempt entities | Diplomatic missions (with tax exemption certificate), UN agencies, INGOs with bilateral agreements |
| Tax authority | Yemen Tax Authority (YTA), Ministry of Finance |
| Reporting | Monthly WHT summary report to YTA by 15th of following month |
| Invoice requirement | WHT amount must be shown separately as deduction |
| Currency | Tax calculated in invoice currency |

**Tax Invoice Format:**
```
[COMPANY LETTERHEAD]
TAX INVOICE

Invoice No: INV-2026-001234
Date: 15 June 2026
Customer: [Company Name]
TIN: [Customer TIN]
Vendor TIN: [ISP TIN]

Description                          Amount (USD)
─────────────────────────────────────────────
DIA 100 Mbps - June 2026              1,000.00
Static IP - June 2026                    30.00
Setup Fee (one-time)                     50.00
─────────────────────────────────────────────
Subtotal                              1,080.00
Withholding Tax (5%)                    (54.00)
─────────────────────────────────────────────
Total Due                             1,026.00

Note: The withholding tax amount must be deducted and remitted to the 
Yemen Tax Authority (YTA) by the customer.
```

### 13.6 Discount Compounding Rules

(Refer to [Section 7.4](#74-discounts--promotions) for detailed rules.)

**Summary:**
1. Discount application order: Volume → Contract Term → Bundle → Promotional → Loyalty
2. Maximum 2 discount types active per subscription
3. Total discount capped at 50% of base recurring charge
4. Prorated on mid-cycle application or removal
5. Promotional discounts cannot be combined with other promotions
6. Referral credits are account credits (non-monetary), not subject to stacking limits

### 13.7 Contract Management Rules

| Rule | Description |
|---|---|
| Minimum Billing Commitment (MBC) | Enterprise only: minimum monthly spend over contract term |
| Contract extensions | Upgrades extend contract by 6 months; downgrades not allowed during contract term |
| Early Termination Fee | 50% of remaining contract value (fees waived if moving to higher-value service) |
| Auto-renewal | Contracts auto-renew month-to-month after term unless 30-day notice given |
| Cool-off period | 7 days from order acceptance (residential right of withdrawal per Yemen consumer law) |

### 13.8 Refund & Credit Policy

| Scenario | Policy |
|---|---|
| Service not delivered within SLA | Full refund of installation fee + 1 month free |
| Service outage > 24 hours | 1 day credit per full day of outage (max 30 days) |
| Billing error (overcharge) | Full refund (to payment method or account credit) |
| Customer cancellation within cool-off period | Full refund of all charges |
| Prepaid service cancellation | Remaining balance refunded (minus 10% admin fee) |
| Domain registration cancellation | No refund (domain is in use / non-returnable) |
| Credit expiration | Account credits expire after 12 months of inactivity |

### 13.9 Data Retention & Privacy

| Data Type | Retention Period | Compliance |
|---|---|---|
| Customer PII (name, ID, address) | Duration of relationship + 3 years | Yemen Data Protection Law (if enacted) / GDPR principles |
| Billing records | 10 years | Yemen Tax Authority requirement |
| Call records (CDRs) | 12 months | Telecom regulatory requirement |
| Network logs | 6 months | Security best practice |
| Payment card data | Not stored (tokenized via gateway) | PCI DSS |
| Deleted accounts | 3 years archived, then destroyed | Internal policy |
| Session data (portal) | 90 days | Internal policy |

### 13.10 Reporting & Audit Requirements

| Report | Frequency | Audience |
|---|---|---|
| Revenue by service type | Daily | Finance, C-level |
| MRR / ARPU / Churn | Monthly | C-level, Board |
| Accounts Receivable Aging | Weekly | Finance, Collections |
| Dunning Funnel | Daily | Collections |
| Provisioning SLA compliance | Weekly | Operations |
| Network uptime / SLA | Monthly | Operations, Enterprise customers |
| Tax summary (WHT) | Monthly | Finance, Tax authority |
| Fraud detection | Real-time + Daily | Security, Revenue Assurance |
| Inventory utilization | Weekly | Planning, Operations |
| Customer acquisition cost | Monthly | Marketing, C-level |
| Leading indicators (calls, emails, portal logins) | Daily | Customer Experience |

**Audit Trail:**
- Every state change in customer lifecycle, order lifecycle, and billing lifecycle must be logged with:
  - Timestamp (UTC +3, Yemen timezone)
  - Actor (system, user ID, API key)
  - Previous state
  - New state
  - Reason/trigger
  - IP address (for user-initiated actions)
- Audit logs are immutable and retained for 7 years

---

## 14. Glossary

| Term | Definition |
|---|---|
| ARPU | Average Revenue Per Unit (per subscriber) |
| BNG | Broadband Network Gateway |
| BRAS | Broadband Remote Access Server |
| BSS | Business Support System |
| CDR | Call Detail Record |
| CIR | Committed Information Rate |
| CPE | Customer Premises Equipment |
| CR | Commercial Registration (Yemen business license) |
| CRM | Customer Relationship Management |
| DCIM | Data Center Infrastructure Management |
| DIA | Dedicated Internet Access |
| DID | Direct Inward Dialing (telephone number) |
| DSLAM | Digital Subscriber Line Access Multiplexer |
| E-LINE | Point-to-point Ethernet service (MEF) |
| E-LAN | Multipoint-to-multipoint Ethernet service (MEF) |
| E-TREE | Rooted-multipoint Ethernet service (MEF) |
| EIR | Excess Information Rate |
| ETF | Early Termination Fee |
| EVC | Ethernet Virtual Connection |
| FUP | Fair Usage Policy |
| GPON | Gigabit Passive Optical Network |
| IAM | Identity and Access Management |
| ISP | Internet Service Provider |
| MBC | Minimum Billing Commitment |
| MEF | Metro Ethernet Forum |
| MRR | Monthly Recurring Revenue |
| MTTR | Mean Time To Repair |
| OLT | Optical Line Terminal |
| ONT/ONU | Optical Network Terminal / Optical Network Unit |
| OSS | Operations Support System |
| PIR | Peak Information Rate |
| PRI | Primary Rate Interface (ISDN E1 for voice) |
| PtMP | Point to MultiPoint |
| PVC | Permanent Virtual Circuit |
| QoS | Quality of Service |
| RADIUS | Remote Authentication Dial-In User Service |
| SBC | Session Border Controller |
| SMB | Small and Medium Business |
| TIN | Taxpayer Identification Number (Yemen: الرقم الضريبي) |
| VLAN | Virtual Local Area Network |
| VPS | Virtual Private Server |
| WHT | Withholding Tax (5% in Yemen) |
| YTA | Yemen Tax Authority (مصلحة الضرائب اليمنية) |
| YER | Yemeni Rial (ISO 4217) |
| .ye | Yemen country code top-level domain (ccTLD) |

---

*Document Version 1.0 — 2026-06-20*
*Author: Product / Business Requirements Team*
*Status: Draft for Review*
