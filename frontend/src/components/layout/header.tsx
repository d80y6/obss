"use client"

import { BreadcrumbBuilder } from "@/components/shared/BreadcrumbBuilder"
import { Bell } from "lucide-react"
import { Button } from "@/components/ui/button"

export function Header() {
  return (
    <header className="flex h-14 items-center gap-4 border-b bg-background px-6">
      <BreadcrumbBuilder />

      <div className="ml-auto flex items-center gap-3">
        <Button variant="ghost" size="icon" className="relative">
          <Bell className="h-4 w-4" />
          <span className="absolute -right-0.5 -top-0.5 flex h-4 w-4 items-center justify-center rounded-full bg-destructive text-[10px] font-medium text-destructive-foreground">
            3
          </span>
        </Button>
        <div className="flex items-center gap-2 pl-2 border-l">
          <div className="flex h-8 w-8 items-center justify-center rounded-full bg-primary text-sm font-medium text-primary-foreground">
            JD
          </div>
        </div>
      </div>
    </header>
  )
}
