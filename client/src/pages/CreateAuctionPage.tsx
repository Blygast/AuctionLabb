import { useState, type FormEvent } from 'react';
import { useNavigate } from 'react-router-dom';
import { auctionService } from '../services/auctionService';
import { getErrorMessage } from '../utils/errors';
import { ROUTES } from '../routes';
import FileDropzone from '../components/FileDropzone';

const ALLOWED_TYPES = new Set([
  'image/jpeg', 'image/png', 'image/gif', 'image/webp', 'image/bmp', 'image/svg+xml',
  'video/mp4', 'video/webm', 'video/avi', 'video/quicktime', 'video/x-matroska',
  'application/pdf',
  'application/msword',
  'application/vnd.openxmlformats-officedocument.wordprocessingml.document',
  'application/vnd.ms-excel',
  'application/vnd.openxmlformats-officedocument.spreadsheetml.sheet',
  'application/vnd.ms-powerpoint',
  'application/vnd.openxmlformats-officedocument.presentationml.presentation',
  'text/plain', 'text/csv',
  'application/rtf',
  'application/vnd.oasis.opendocument.text',
  'application/vnd.oasis.opendocument.spreadsheet',
  'application/zip',
  'application/vnd.rar',
]);

const ALLOWED_EXTENSIONS = new Set([
  'jpg', 'jpeg', 'png', 'gif', 'webp', 'bmp', 'svg',
  'mp4', 'webm', 'avi', 'mov', 'mkv',
  'pdf', 'doc', 'docx', 'xls', 'xlsx', 'ppt', 'pptx',
  'txt', 'csv', 'rtf', 'odt', 'ods', 'zip', 'rar',
]);

const MAX_FILE_SIZE = 50 * 1024 * 1024;

interface FormState {
  title: string;
  description: string;
  startingPrice: string;
  startDate: string;
  endDate: string;
  files: File[];
}

const EMPTY: FormState = {
  title: '', description: '', startingPrice: '', startDate: '', endDate: '', files: [],
};

const ErrorBanner = ({ message }: { message: string }) => (
  <div className="flex items-center p-4 mb-4 text-sm text-red-800 border border-red-300 rounded-lg bg-red-50 dark:bg-gray-800 dark:text-red-400 dark:border-red-800">
    <svg className="shrink-0 inline w-4 h-4 me-3" fill="currentColor" viewBox="0 0 20 20"><path fillRule="evenodd" d="M18 10a8 8 0 11-16 0 8 8 0 0116 0zm-7-4a1 1 0 11-2 0 1 1 0 012 0zM9 9a1 1 0 000 2v3a1 1 0 001 1h1a1 1 0 100-2v-3a1 1 0 00-1-1H9z" clipRule="evenodd" /></svg>
    {message}
  </div>
);

export default function CreateAuctionPage() {
  const navigate = useNavigate();
  const [form, setForm] = useState<FormState>(EMPTY);
  const [error, setError] = useState('');
  const [loading, setLoading] = useState(false);

  const update = <K extends keyof FormState>(k: K, v: FormState[K]) => setForm((f) => ({ ...f, [k]: v }));

  const handleSubmit = async (e: FormEvent) => {
    e.preventDefault();
    setError(''); setLoading(true);
    try {
      await auctionService.create({
        title: form.title,
        description: form.description,
        startingPrice: parseFloat(form.startingPrice),
        startDate: form.startDate,
        endDate: form.endDate,
        files: form.files,
      });
      navigate(ROUTES.auctions);
    } catch (err: unknown) {
      setError(getErrorMessage(err, 'Failed to create auction.'));
    } finally { setLoading(false); }
  };

  const minStartDate = new Date().toISOString().slice(0, 16);

  return (
    <div className="py-6">
      <div className="mb-6">
        <h1 className="text-2xl font-semibold text-gray-900 dark:text-white">Create Auction</h1>
        <p className="text-sm text-gray-500 dark:text-gray-400 mt-1">List a new item for auction</p>
      </div>

      {error && <ErrorBanner message={error} />}

      <div className="bg-white dark:bg-gray-800 relative shadow-md sm:rounded-lg">
        <div className="p-6">
          <form onSubmit={handleSubmit} className="space-y-6">
            <div className="grid grid-cols-1 md:grid-cols-2 gap-6">
              <div className="md:col-span-2">
                <label className="block mb-2 text-sm font-medium text-gray-900 dark:text-white">Title</label>
                <input type="text" value={form.title} onChange={(e) => update('title', e.target.value)} required maxLength={200}
                  className="bg-gray-50 border border-gray-300 text-gray-900 text-sm rounded-lg focus:ring-primary-500 focus:border-primary-500 block w-full p-2.5 dark:bg-gray-700 dark:border-gray-600 dark:placeholder-gray-400 dark:text-white"
                  placeholder="What are you auctioning?" />
              </div>
              <div className="md:col-span-2">
                <label className="block mb-2 text-sm font-medium text-gray-900 dark:text-white">Description</label>
                <textarea value={form.description} onChange={(e) => update('description', e.target.value)} required maxLength={2000} rows={4}
                  className="bg-gray-50 border border-gray-300 text-gray-900 text-sm rounded-lg focus:ring-primary-500 focus:border-primary-500 block w-full p-2.5 dark:bg-gray-700 dark:border-gray-600 dark:placeholder-gray-400 dark:text-white"
                  placeholder="Describe your item in detail..." />
              </div>
              <div>
                <label className="block mb-2 text-sm font-medium text-gray-900 dark:text-white">Starting Price ($)</label>
                <input type="number" step="0.01" min="0.01" value={form.startingPrice} onChange={(e) => update('startingPrice', e.target.value)} required
                  className="bg-gray-50 border border-gray-300 text-gray-900 text-sm rounded-lg focus:ring-primary-500 focus:border-primary-500 block w-full p-2.5 dark:bg-gray-700 dark:border-gray-600 dark:placeholder-gray-400 dark:text-white"
                  placeholder="0.00" />
              </div>
              <div>
                <label className="block mb-2 text-sm font-medium text-gray-900 dark:text-white">Start Date</label>
                <input type="datetime-local" value={form.startDate} onChange={(e) => update('startDate', e.target.value)} required min={minStartDate}
                  className="bg-gray-50 border border-gray-300 text-gray-900 text-sm rounded-lg focus:ring-primary-500 focus:border-primary-500 block w-full p-2.5 dark:bg-gray-700 dark:border-gray-600 dark:placeholder-gray-400 dark:text-white" />
              </div>
              <div>
                <label className="block mb-2 text-sm font-medium text-gray-900 dark:text-white">End Date</label>
                <input type="datetime-local" value={form.endDate} onChange={(e) => update('endDate', e.target.value)} required min={form.startDate || minStartDate}
                  className="bg-gray-50 border border-gray-300 text-gray-900 text-sm rounded-lg focus:ring-primary-500 focus:border-primary-500 block w-full p-2.5 dark:bg-gray-700 dark:border-gray-600 dark:placeholder-gray-400 dark:text-white" />
              </div>
            </div>

            <div className="border-t border-gray-200 dark:border-gray-700 pt-6">
              <FileDropzone
                files={form.files}
                onChange={(f) => update('files', f)}
                accept={{ types: ALLOWED_TYPES, extensions: ALLOWED_EXTENSIONS }}
                maxBytes={MAX_FILE_SIZE}
              />
            </div>

            <div className="flex items-center gap-3 pt-4 border-t border-gray-200 dark:border-gray-700">
              <button type="submit" disabled={loading}
                className="text-white bg-primary-700 hover:bg-primary-800 focus:ring-4 focus:ring-primary-300 font-medium rounded-lg text-sm px-5 py-2.5 dark:bg-primary-600 dark:hover:bg-primary-700 dark:focus:ring-primary-800 disabled:bg-primary-400">
                {loading ? 'Creating...' : 'Create Auction'}
              </button>
              <button type="button" onClick={() => navigate(ROUTES.auctions)}
                className="text-gray-900 bg-white border border-gray-300 focus:outline-none hover:bg-gray-100 focus:ring-4 focus:ring-gray-200 font-medium rounded-lg text-sm px-5 py-2.5 dark:bg-gray-800 dark:text-white dark:border-gray-600 dark:hover:bg-gray-700 dark:hover:border-gray-600 dark:focus:ring-gray-700">
                Cancel
              </button>
            </div>
          </form>
        </div>
      </div>
    </div>
  );
}
