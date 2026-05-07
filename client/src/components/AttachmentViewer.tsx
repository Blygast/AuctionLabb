interface Attachment {
  id: number;
  fileName: string;
  contentType: string;
  fileSize: number;
  uploadedAt: string;
  url: string;
}

interface AttachmentViewerProps {
  attachments: Attachment[];
}

function isImage(ct: string) {
  return ct.startsWith('image/');
}

function isVideo(ct: string) {
  return ct.startsWith('video/');
}

function formatSize(bytes: number) {
  if (bytes < 1024) return `${bytes} B`;
  if (bytes < 1024 * 1024) return `${(bytes / 1024).toFixed(1)} KB`;
  return `${(bytes / (1024 * 1024)).toFixed(1)} MB`;
}

function fileIcon(ct: string) {
  if (isImage(ct)) {
    return (
      <svg className="w-8 h-8" fill="none" stroke="currentColor" viewBox="0 0 24 24">
        <path strokeLinecap="round" strokeLinejoin="round" strokeWidth="2" d="M4 16l4.586-4.586a2 2 0 012.828 0L16 16m-2-2l1.586-1.586a2 2 0 012.828 0L20 14m-6-6h.01M6 20h12a2 2 0 002-2V6a2 2 0 00-2-2H6a2 2 0 00-2 2v12a2 2 0 002 2z" />
      </svg>
    );
  }
  if (isVideo(ct)) {
    return (
      <svg className="w-8 h-8" fill="none" stroke="currentColor" viewBox="0 0 24 24">
        <path strokeLinecap="round" strokeLinejoin="round" strokeWidth="2" d="M15 10l4.553-2.276A1 1 0 0121 8.618v6.764a1 1 0 01-1.447.894L15 14M5 18h8a2 2 0 002-2V8a2 2 0 00-2-2H5a2 2 0 00-2 2v8a2 2 0 002 2z" />
      </svg>
    );
  }
  return (
    <svg className="w-8 h-8" fill="none" stroke="currentColor" viewBox="0 0 24 24">
      <path strokeLinecap="round" strokeLinejoin="round" strokeWidth="2" d="M9 12h6m-6 4h6m2 5H7a2 2 0 01-2-2V5a2 2 0 012-2h5.586a1 1 0 01.707.293l5.414 5.414a1 1 0 01.293.707V19a2 2 0 01-2 2z" />
    </svg>
  );
}

const API_BASE = 'https://localhost:5001';

export default function AttachmentViewer({ attachments }: AttachmentViewerProps) {
  if (attachments.length === 0) return null;

  const images = attachments.filter((a) => isImage(a.contentType));
  const videos = attachments.filter((a) => isVideo(a.contentType));
  const documents = attachments.filter((a) => !isImage(a.contentType) && !isVideo(a.contentType));

  return (
    <div className="space-y-6">
      {images.length > 0 && (
        <div>
          <h4 className="text-sm font-medium text-gray-900 dark:text-white mb-3">Images</h4>
          <div className="grid grid-cols-2 md:grid-cols-3 lg:grid-cols-4 gap-3">
            {images.map((att) => (
              <a
                key={att.id}
                href={`${API_BASE}${att.url}`}
                target="_blank"
                rel="noopener noreferrer"
                className="group relative block overflow-hidden rounded-lg border border-gray-200 dark:border-gray-700 bg-gray-100 dark:bg-gray-700 hover:ring-2 hover:ring-primary-500"
              >
                <img
                  src={`${API_BASE}${att.url}`}
                  alt={att.fileName}
                  className="w-full h-32 object-cover"
                />
                <div className="absolute inset-0 bg-black/0 group-hover:bg-black/20 transition-colors flex items-end">
                  <span className="text-xs text-white bg-black/50 px-2 py-1 truncate w-full opacity-0 group-hover:opacity-100 transition-opacity">
                    {att.fileName}
                  </span>
                </div>
              </a>
            ))}
          </div>
        </div>
      )}

      {videos.length > 0 && (
        <div>
          <h4 className="text-sm font-medium text-gray-900 dark:text-white mb-3">Videos</h4>
          <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
            {videos.map((att) => (
              <div key={att.id} className="rounded-lg overflow-hidden border border-gray-200 dark:border-gray-700">
                <video
                  controls
                  className="w-full max-h-64"
                  preload="metadata"
                >
                  <source src={`${API_BASE}${att.url}`} type={att.contentType} />
                </video>
                <div className="p-2 bg-gray-50 dark:bg-gray-700 flex items-center justify-between">
                  <span className="text-xs text-gray-500 dark:text-gray-400 truncate">{att.fileName}</span>
                  <a
                    href={`${API_BASE}${att.url}`}
                    download
                    className="text-xs text-primary-600 hover:underline dark:text-primary-500"
                  >
                    Download
                  </a>
                </div>
              </div>
            ))}
          </div>
        </div>
      )}

      {documents.length > 0 && (
        <div>
          <h4 className="text-sm font-medium text-gray-900 dark:text-white mb-3">Documents</h4>
          <div className="space-y-2">
            {documents.map((att) => (
              <a
                key={att.id}
                href={`${API_BASE}${att.url}`}
                target="_blank"
                rel="noopener noreferrer"
                className="flex items-center p-3 bg-gray-50 rounded-lg border border-gray-200 dark:bg-gray-700 dark:border-gray-600 hover:bg-gray-100 dark:hover:bg-gray-600 transition-colors"
              >
                <span className="text-gray-400 dark:text-gray-500 mr-3">
                  {fileIcon(att.contentType)}
                </span>
                <div className="flex-1 min-w-0">
                  <p className="text-sm font-medium text-gray-900 dark:text-white truncate">{att.fileName}</p>
                  <p className="text-xs text-gray-500 dark:text-gray-400">{formatSize(att.fileSize)}</p>
                </div>
                <svg className="w-4 h-4 text-gray-400 dark:text-gray-500 ml-2 shrink-0" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                  <path strokeLinecap="round" strokeLinejoin="round" strokeWidth="2" d="M12 10v6m0 0l-3-3m3 3l3-3m2 8H7a2 2 0 01-2-2V5a2 2 0 012-2h5.586a1 1 0 01.707.293l5.414 5.414a1 1 0 01.293.707V19a2 2 0 01-2 2z" />
                </svg>
              </a>
            ))}
          </div>
        </div>
      )}
    </div>
  );
}

export type { Attachment };
export { isImage, isVideo };
export const ATTACHMENT_API_BASE = API_BASE;
