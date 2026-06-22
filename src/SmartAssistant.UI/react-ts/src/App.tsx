import React, { useState, useRef } from 'react';

type Message = { role: 'user' | 'ai'; text: string; sources?: string[] };

const API_URL = import.meta.env.VITE_API_URL || 'http://localhost:5069';

export default function App() {
  const [file, setFile] = useState<File | null>(null);
  const [documentId, setDocumentId] = useState<string | null>(null);
  const [isUploading, setIsUploading] = useState(false);
  const [uploadError, setUploadError] = useState('');
  const [isDragActive, setIsDragActive] = useState(false);

  const [messages, setMessages] = useState<Message[]>([]);
  const [question, setQuestion] = useState('');
  const [isAsking, setIsAsking] = useState(false);
  const [chatError, setChatError] = useState('');

  const fileInputRef = useRef<HTMLInputElement>(null);
  const chatEndRef = useRef<HTMLDivElement>(null);

  const scrollToBottom = () => {
    chatEndRef.current?.scrollIntoView({ behavior: 'smooth' });
  };

  const handleDrag = (e: React.DragEvent) => {
    e.preventDefault();
    e.stopPropagation();
    if (e.type === 'dragenter' || e.type === 'dragover') setIsDragActive(true);
    else if (e.type === 'dragleave') setIsDragActive(false);
  };

  const handleDrop = (e: React.DragEvent) => {
    e.preventDefault();
    e.stopPropagation();
    setIsDragActive(false);
    if (e.dataTransfer.files && e.dataTransfer.files[0]) {
      validateAndSetFile(e.dataTransfer.files[0]);
    }
  };

  const handleFileChange = (e: React.ChangeEvent<HTMLInputElement>) => {
    if (e.target.files && e.target.files[0]) {
      validateAndSetFile(e.target.files[0]);
    }
  };

  const validateAndSetFile = (selected: File) => {
    if (selected.type !== 'application/pdf') {
      setUploadError('Only PDF files are allowed.');
      return;
    }
    setFile(selected);
    setUploadError('');
  };

  const handleUpload = async () => {
    if (!file) return;
    setIsUploading(true);
    setUploadError('');
    setDocumentId(null);
    setMessages([]);

    const formData = new FormData();
    formData.append('file', file);

    try {
      const res = await fetch(`${API_URL}/api/documents/upload`, {
        method: 'POST',
        body: formData,
      });
      if (!res.ok) throw new Error('Upload failed');
      const data = await res.json();
      setDocumentId(data.documentId);
    } catch (err: any) {
      setUploadError(err.message || 'An error occurred during upload.');
    } finally {
      setIsUploading(false);
    }
  };

  const handleAsk = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!question.trim() || !documentId) return;

    const currentQ = question;
    setMessages((prev) => [...prev, { role: 'user', text: currentQ }]);
    setQuestion('');
    setIsAsking(true);
    setChatError('');
    setTimeout(scrollToBottom, 100);

    try {
      const res = await fetch(`${API_URL}/api/chat/ask`, {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({ documentId, question: currentQ }),
      });
      if (!res.ok) throw new Error('Failed to get answer');
      const data = await res.json();
      setMessages((prev) => [
        ...prev,
        { role: 'ai', text: data.answer, sources: data.sources },
      ]);
    } catch (err: any) {
      setChatError(err.message || 'Error fetching answer.');
      setMessages((prev) => [...prev, { role: 'ai', text: 'Error getting response.' }]);
    } finally {
      setIsAsking(false);
      setTimeout(scrollToBottom, 100);
    }
  };

  return (
    <div className="flex h-screen bg-[#0A0F1E] text-white font-sans selection:bg-[#6366F1] selection:text-white">
      {/* Sidebar */}
      <div className="w-80 bg-[#111827] border-r border-[#1F2937] flex flex-col p-6 shadow-2xl z-10">
        <h1 className="text-xl font-bold mb-8 flex items-center gap-2 text-[#E5E7EB]">
          <div className="w-3 h-3 rounded-full bg-[#6366F1] shadow-[0_0_8px_#6366F1]"></div>
          Smart AI Assistant
        </h1>

        <div className="mb-6">
          <label className="text-sm font-semibold text-gray-400 uppercase tracking-wider mb-3 block">
            Document Context
          </label>
          
          {/* Drag & Drop Area */}
          <div
            onDragEnter={handleDrag}
            onDragLeave={handleDrag}
            onDragOver={handleDrag}
            onDrop={handleDrop}
            className={`border-2 border-dashed rounded-xl p-6 text-center transition-all duration-300 cursor-pointer flex flex-col items-center justify-center gap-3
${isDragActive ? 'border-[#6366F1] bg-[#6366F1]/10' : 'border-gray-700 hover:border-gray-500 hover:bg-[#1F2937]/50'}
${documentId ? 'hidden' : 'block'}`}
            onClick={() => fileInputRef.current?.click()}
          >
            <input
              type="file"
              accept=".pdf"
              className="hidden"
              ref={fileInputRef}
              onChange={handleFileChange}
            />
            <svg className="w-8 h-8 text-gray-400" fill="none" viewBox="0 0 24 24" stroke="currentColor">
              <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={1.5} d="M7 16a4 4 0 01-.88-7.903A5 5 0 1115.9 6L16 6a5 5 0 011 9.9M15 13l-3-3m0 0l-3 3m3-3v12" />
            </svg>
            <div className="text-sm text-gray-300">
              <span className="text-[#6366F1] font-medium">Click to upload</span> or drag and drop
            </div>
            <p className="text-xs text-gray-500">PDF up to 10MB</p>
          </div>

          {file && !documentId && (
            <div className="mt-4 p-4 rounded-lg bg-[#1F2937] border border-gray-700">
              <p className="text-sm truncate text-gray-300" title={file.name}>{file.name}</p>
              <button
                onClick={handleUpload}
                disabled={isUploading}
                className="w-full mt-3 bg-[#6366F1] hover:bg-[#4F46E5] text-white py-2 rounded-lg transition-colors disabled:opacity-50 text-sm font-medium"
              >
                {isUploading ? 'Uploading & Processing...' : 'Process Document'}
              </button>
            </div>
          )}

          {uploadError && <p className="text-red-400 mt-3 text-sm">{uploadError}</p>}

          {/* Active Document Card */}
          {documentId && (
            <div className="mt-2 p-4 rounded-xl bg-[#1F2937] border border-[#6366F1]/30 shadow-[0_0_15px_rgba(99,102,241,0.05)]">
              <div className="flex items-center gap-3">
                <div className="w-10 h-10 rounded-lg bg-[#6366F1]/20 flex items-center justify-center text-[#6366F1]">
                  <svg className="w-5 h-5" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                    <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M9 12h6m-6 4h6m2 5H7a2 2 0 01-2-2V5a2 2 0 012-2h5.586a1 1 0 01.707.293l5.414 5.414a1 1 0 01.293.707V19a2 2 0 01-2 2z" />
                  </svg>
                </div>
                <div className="overflow-hidden">
                  <div className="flex items-center gap-2">
                    <div className="w-2 h-2 rounded-full bg-green-500 animate-pulse"></div>
                    <span className="text-xs font-semibold text-green-400 uppercase">Active</span>
                  </div>
                  <p className="text-sm font-medium text-gray-200 truncate" title={file?.name}>{file?.name}</p>
                </div>
              </div>
              <button 
                onClick={() => { setDocumentId(null); setFile(null); setMessages([]); }}
                className="mt-4 w-full text-xs text-gray-400 hover:text-white transition-colors"
              >
                Upload new document
              </button>
            </div>
          )}
        </div>
      </div>

      {/* Chat Area */}
      <div className="flex-1 flex flex-col relative">
        <div className="flex-1 overflow-y-auto p-8 space-y-6 scroll-smooth">
          {messages.length === 0 && (
            <div className="h-full flex flex-col items-center justify-center text-gray-500 space-y-4">
              <div className="w-16 h-16 rounded-full bg-[#111827] flex items-center justify-center border border-gray-800 shadow-xl">
                <svg className="w-8 h-8 text-[#6366F1]" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                  <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={1.5} d="M8 10h.01M12 10h.01M16 10h.01M9 16H5a2 2 0 01-2-2V6a2 2 0 012-2h14a2 2 0 012 2v8a2 2 0 01-2 2h-5l-5 5v-5z" />
                </svg>
              </div>
              <p className="text-lg">
                {documentId ? 'I am ready. Ask me anything about the document.' : 'Awaiting document upload...'}
              </p>
            </div>
          )}
          
          {messages.map((msg, idx) => (
            <div key={idx} className={`flex ${msg.role === 'user' ? 'justify-end' : 'justify-start'}`}>
              <div className={`max-w-[75%] p-5 rounded-2xl ${
  msg.role === 'user'
      ? 'bg-[#6366F1] text-white rounded-br-none shadow-[0_4px_20px_rgba(99,102,241,0.3)]'
      : 'bg-[#111827] border border-[#1F2937] text-gray-200 rounded-bl-none shadow-[0_0_15px_rgba(99,102,241,0.05)]'
}`}>
                <div className="prose prose-invert max-w-none text-sm leading-relaxed whitespace-pre-wrap">
                  {msg.text}
                </div>
                
                {msg.sources && msg.sources.length > 0 && (
                  <details className="mt-4 text-xs text-gray-400 group">
                    <summary className="font-medium cursor-pointer flex items-center gap-2 hover:text-[#6366F1] transition-colors select-none">
                      <svg className="w-4 h-4 transform group-open:rotate-90 transition-transform" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                        <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M9 5l7 7-7 7" />
                      </svg>
                      View Sources ({msg.sources.length})
                    </summary>
                    <div className="mt-3 space-y-2 p-3 bg-[#0A0F1E] rounded-lg border border-[#1F2937]">
                      {msg.sources.map((s, i) => (
                        <div key={i} className="pl-2 border-l-2 border-[#6366F1]/50 line-clamp-3 hover:line-clamp-none transition-all">
                          {s}
                        </div>
                      ))}
                    </div>
                  </details>
                )}
              </div>
            </div>
          ))}

          {isAsking && (
            <div className="flex justify-start">
              <div className="bg-[#111827] border border-[#1F2937] p-5 rounded-2xl rounded-bl-none flex items-center gap-2 shadow-[0_0_15px_rgba(99,102,241,0.1)]">
                <div className="w-2 h-2 rounded-full bg-[#6366F1] animate-bounce" style={{ animationDelay: '0ms' }}></div>
                <div className="w-2 h-2 rounded-full bg-[#6366F1] animate-bounce" style={{ animationDelay: '150ms' }}></div>
                <div className="w-2 h-2 rounded-full bg-[#6366F1] animate-bounce" style={{ animationDelay: '300ms' }}></div>
              </div>
            </div>
          )}
          {chatError && <div className="text-red-400 text-sm text-center">{chatError}</div>}
          <div ref={chatEndRef} />
        </div>
        
        {/* Input Area */}
        <div className="p-6 bg-[#0A0F1E] border-t border-[#1F2937]">
          <form onSubmit={handleAsk} className="relative max-w-4xl mx-auto">
            <input
              type="text"
              value={question}
              onChange={(e) => setQuestion(e.target.value)}
              placeholder={documentId ? "Type your question..." : "Upload a document first..."}
              disabled={!documentId || isAsking}
              className="w-full bg-[#111827] border border-[#1F2937] rounded-xl px-5 py-4 pr-16 text-sm focus:outline-none focus:border-[#6366F1] focus:ring-1 focus:ring-[#6366F1] disabled:opacity-50 transition-all placeholder-gray-500"
            />
            <button
              type="submit"
              disabled={!documentId || isAsking || !question.trim()}
              className="absolute right-2 top-2 bottom-2 bg-[#6366F1] hover:bg-[#4F46E5] text-white p-2 rounded-lg disabled:opacity-50 transition-colors flex items-center justify-center w-12"
            >
              <svg className="w-5 h-5 ml-1" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M12 19l9 2-9-18-9 18 9-2zm0 0v-8" />
              </svg>
            </button>
          </form>
        </div>
      </div>
    </div>
  );
}