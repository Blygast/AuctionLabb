import { Component, type ErrorInfo, type ReactNode } from 'react';

interface Props {
  children: ReactNode;
  fallback?: ReactNode;
}

interface State {
  error: Error | null;
}

/**
 * Catches any unhandled error thrown during render or in a child component's
 * lifecycle, so the whole app doesn't white-screen. Render an error UI
 * with a reload button instead.
 */
export class ErrorBoundary extends Component<Props, State> {
  state: State = { error: null };

  static getDerivedStateFromError(error: Error): State {
    return { error };
  }

  componentDidCatch(error: Error, info: ErrorInfo): void {
    // In a real app this would go to your error reporter (Sentry, etc.)
    console.error('Uncaught error in component tree:', error, info);
  }

  private handleReload = () => {
    window.location.reload();
  };

  render() {
    if (this.state.error) {
      if (this.props.fallback) return this.props.fallback;
      return (
        <div className="flex flex-col items-center justify-center min-h-screen p-6 text-center">
          <h1 className="text-2xl font-semibold text-gray-900 dark:text-white mb-2">
            Something went wrong
          </h1>
          <p className="text-sm text-gray-500 dark:text-gray-400 mb-6 max-w-md">
            {this.state.error.message}
          </p>
          <button
            onClick={this.handleReload}
            className="text-white bg-primary-700 hover:bg-primary-800 font-medium rounded-lg text-sm px-5 py-2.5"
          >
            Reload the page
          </button>
        </div>
      );
    }
    return this.props.children;
  }
}
