"use client"

import { Input } from "@/components/ui/input"
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from "@/components/ui/select"
import { cn } from "@/lib/utils"
import { FieldError, UseFormRegisterReturn } from "react-hook-form"

interface FormFieldProps {
  label: string
  error?: FieldError
  required?: boolean
  children?: React.ReactNode
  registration?: UseFormRegisterReturn
  type?: string
  placeholder?: string
  className?: string
}

export function FormField({ label, error, required, children, registration, type, placeholder, className }: FormFieldProps) {
  return (
    <div className={cn("space-y-2", className)}>
      <label className="text-sm font-medium">
        {label}
        {required && <span className="text-destructive ml-1">*</span>}
      </label>
      {children || (
        <Input
          type={type || "text"}
          placeholder={placeholder}
          {...registration}
        />
      )}
      {error && (
        <p className="text-xs text-destructive">{error.message}</p>
      )}
    </div>
  )
}

interface FormSelectFieldProps extends Omit<FormFieldProps, "children"> {
  options: { label: string; value: string }[]
  value: string
  onValueChange: (value: string) => void
  placeholder?: string
}

export function FormSelectField({ label, error, required, options, value, onValueChange, placeholder, className }: FormSelectFieldProps) {
  return (
    <div className={cn("space-y-2", className)}>
      <label className="text-sm font-medium">
        {label}
        {required && <span className="text-destructive ml-1">*</span>}
      </label>
      <Select value={value} onValueChange={onValueChange}>
        <SelectTrigger>
          <SelectValue placeholder={placeholder} />
        </SelectTrigger>
        <SelectContent>
          {options.map((opt) => (
            <SelectItem key={opt.value} value={opt.value}>
              {opt.label}
            </SelectItem>
          ))}
        </SelectContent>
      </Select>
      {error && (
        <p className="text-xs text-destructive">{error.message}</p>
      )}
    </div>
  )
}
