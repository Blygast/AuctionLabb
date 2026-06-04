import Spinner from './Spinner';

interface Props {
  label?: string;
}

/** Full-viewport loading indicator used by route guards. */
export default function FullPageSpinner({ label = 'Loading...' }: Props) {
  return (
    <div className="flex flex-col items-center justify-center min-h-screen gap-3">
      <Spinner size="lg" />
      <span className="text-sm text-gray-500 dark:text-gray-400">{label}</span>
    </div>
  );
}
