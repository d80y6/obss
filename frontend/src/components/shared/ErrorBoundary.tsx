"use client"

import { Component, ErrorInfo, ReactNode } from "react"

interface Props {
  children: ReactNode
  fallback?: ReactNode
}

interface State {
  hasError: boolean
  error?: Error
}

export class ErrorBoundary extends Component<Props, State> {
  constructor(props: Props) {
    super(props)
    this.state = { hasError: false }
  }

  static getDerivedStateFromError(error: Error): State {
    return { hasError: true, error }
  }

  componentDidCatch(error: Error, errorInfo: ErrorInfo) {
    console.error("ErrorBoundary caught:", error, errorInfo)
  }

  render() {
    if (this.state.hasError) {
      return this.props.fallback || (
        <div className="flex flex-col items-center justify-center py-12 text-center">
          <h3 className="text-lg font-medium text-red-600">Something went wrong</h3>
          <p className="mt-1 text-sm text-gray-400">{this.state.error?.message}</p>
          <button
            onClick={() => this.setState({ hasError: false, error: undefined })}
            className="mt-4 rounded bg-blue-600 px-4 py-2 text-sm text-white hover:bg-blue-700"
          >
            Try again
          </button>
        </div>
      )
    }
    return this.props.children
  }
}
