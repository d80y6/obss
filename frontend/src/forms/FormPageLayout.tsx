"use client"

import { Button } from "@/components/ui/button"
import { ArrowLeft } from "lucide-react"
import Link from "next/link"
import { FormEventHandler } from "react"

interface FormPageLayoutProps {
  title: string
  backHref: string
  children: React.ReactNode
  onSubmit?: FormEventHandler<HTMLFormElement>
}

export function FormPageLayout({ title, backHref, children, onSubmit }: FormPageLayoutProps) {
  return (
    <div className="flex-1 space-y-6 p-6">
      <div className="flex items-center gap-4">
        <Button variant="ghost" size="icon" asChild>
          <Link href={backHref}>
            <ArrowLeft className="h-4 w-4" />
          </Link>
        </Button>
        <h1 className="text-2xl font-bold tracking-tight">{title}</h1>
      </div>
      <form onSubmit={onSubmit}>
        {children}
      </form>
    </div>
  )
}
