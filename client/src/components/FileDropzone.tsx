import { useRef, type ChangeEvent } from 'react';
import { formatSize } from '../utils/format';

interface Props {
  files: File[];
  onChange: (files: File[]) => void;
  /** File extensions (without dot) and/or MIME types accepted. */
  accept: { types: Set<string>; extensions: Set<string> };
  /** Max bytes per file. */
  maxBytes: number;
  /** Optional error message to show above the dropzone. */
  error?: string;
}

const PaperclipIcon = (
  <svg className="w-4 h-4 text-gray-400 mr-2 shrink-0" fill="none" stroke="currentColor" viewBox="0 0 24 24">
    <path strokeLinecap="round" strokeLinejoin="round" strokeWidth="2" d="M15.172 7l-6.586 6.586a2 2 0 102.828 2.828l6.414-6.586a4 4 0 00-5.656-5.656l-6.415 6.585a6 6 0 108.486 8.486L20.5 13" />
  </svg>
);

const XIcon = (
  <svg className="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
    <path strokeLinecap="round" strokeLinejoin="round" strokeWidth="2" d="M6 18L18 6M6 6l12 12" />
  </svg>
);

const UploadIcon = (
  <svg className="w-8 h-8 mb-2 text-gray-500 dark:text-gray-400" fill="none" stroke="currentColor" viewBox="0 0 24 24">
    <path strokeLinecap="round" strokeLinejoin="round" strokeWidth="2" d="M7 16a4 4 0 01-.88-7.903A5 5 0 1115.9 6L16 6a5 5 0 011 9.9M15 13l-3-3m0 0l-3 3m3-3v12" />
  </svg>
);

export default function FileDropzone({ files, onChange, accept, maxBytes, error }: Props) {
  const inputRef = useRef<HTMLInputElement>(null);

  const handleFiles = (e: ChangeEvent<HTMLInputElement>) => {
    if (!e.target.files) return;
    const accepted: File[] = [];
    for (const f of Array.from(e.target.files)) {
      const ext = f.name.split('.').pop()?.toLowerCase() ?? '';
      if (!accept.types.has(f.type) && !accept.extensions.has(ext)) {
        onChange([]); // signal error by clearing; parent should show its own banner
        return;
      }
      if (f.size > maxBytes) {
        onChange([]);
        return;
      }
      accepted.push(f);
    }
    onChange([...files, ...accepted]);
    if (inputRef.current) inputRef.current.value = '';
  };

  const remove = (i: number) => onChange(files.filter((_, idx) => idx !== i));

  return (
    <div>
      <label className="flex flex-col items-center justify-center w-full h-32 border-2 border-gray-300 border-dashed rounded-lg cursor-pointer bg-gray-50 hover:bg-gray-100 dark:hover:bg-gray-700 dark:bg-gray-700 dark:border-gray-600 dark:hover:border-gray-500 transition-colors">
        <div className="flex flex-col items-center justify-center pt-5 pb-6">
          {UploadIcon}
          <p className="text-sm text-gray-500 dark:text-gray-400">
            <span className="font-semibold">Click to upload</span> or drag and drop
          </p>
          <p className="text-xs text-gray-500 dark:text-gray-400 mt-1">
            Images, videos, and documents (max {Math.round(maxBytes / (1024 * 1024))}MB each)
          </p>
        </div>
        <input ref={inputRef} type="file" multiple onChange={handleFiles} className="hidden" />
      </label>
      {error && <p className="mt-2 text-sm text-red-600 dark:text-red-400">{error}</p>}
      {files.length > 0 && (
        <ul className="mt-3 space-y-2">
          {files.map((file, i) => (
            <li key={i} className="flex items-center justify-between p-2 bg-gray-50 dark:bg-gray-700 rounded-lg border border-gray-200 dark:border-gray-600">
              <div className="flex items-center min-w-0 flex-1">
                {PaperclipIcon}
                <span className="text-sm text-gray-900 dark:text-white truncate">{file.name}</span>
                <span className="text-xs text-gray-500 dark:text-gray-400 ml-2 shrink-0">({formatSize(file.size)})</span>
              </div>
              <button type="button" onClick={() => remove(i)} className="ml-2 text-red-500 hover:text-red-700 dark:text-red-400 dark:hover:text-red-300 shrink-0">
                {XIcon}
              </button>
            </li>
          ))}
        </ul>
      )}
    </div>
  );
}
