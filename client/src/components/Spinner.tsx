interface SpinnerProps {
  size?: 'sm' | 'md' | 'lg';
  className?: string;
}

const SIZE_CLASS: Record<NonNullable<SpinnerProps['size']>, string> = {
  sm: 'h-6 w-6 border-2',
  md: 'h-10 w-10 border-b-2',
  lg: 'h-12 w-12 border-b-2',
};

export default function Spinner({ size = 'md', className = '' }: SpinnerProps) {
  return (
    <div
      role="status"
      aria-label="Loading"
      className={`animate-spin rounded-full border-primary-600 ${SIZE_CLASS[size]} ${className}`}
    />
  );
}
