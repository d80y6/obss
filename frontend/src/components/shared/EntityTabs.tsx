"use client"

import { Tabs, TabsContent, TabsList, TabsTrigger } from "@/components/ui/tabs"

interface Tab {
  id: string
  label: string
  content: React.ReactNode
}

interface EntityTabsProps {
  tabs: Tab[]
  defaultTab?: string
}

export function EntityTabs({ tabs, defaultTab }: EntityTabsProps) {
  return (
    <Tabs defaultValue={defaultTab || tabs[0]?.id}>
      <TabsList>
        {tabs.map((tab) => (
          <TabsTrigger key={tab.id} value={tab.id}>
            {tab.label}
          </TabsTrigger>
        ))}
      </TabsList>
      {tabs.map((tab) => (
        <TabsContent key={tab.id} value={tab.id} className="mt-6">
          {tab.content}
        </TabsContent>
      ))}
    </Tabs>
  )
}
