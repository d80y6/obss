import { SelfCareShell } from "@/components/selfcare/selfcare-shell"

export default function PortalLayout({ children }: { children: React.ReactNode }) {
  return <SelfCareShell>{children}</SelfCareShell>
}
