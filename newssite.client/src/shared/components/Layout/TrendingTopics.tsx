import React, { useEffect, useState } from 'react';
import { useLanguage } from '../../../contexts/LanguageContext';
import { FaTwitter, FaReddit } from 'react-icons/fa';
import { api } from '../../services/api';

interface Topic {
  title: string;
  url: string;
}

interface TrendingTopicsResponse {
  twitterTopics: Topic[];
  redditTopics: Topic[];
}

interface TrendingTopicsProps {
  className?: string;
}

export const TrendingTopics: React.FC<TrendingTopicsProps> = ({ className = '' }) => {
  const { t } = useLanguage();
  const [twitterTopics, setTwitterTopics] = useState<Topic[]>([]);
  const [redditTopics, setRedditTopics] = useState<Topic[]>([]);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    const fetchTopics = async () => {
      try {
        const { data } = await api.get<TrendingTopicsResponse>('/trending');
        setTwitterTopics(data.twitterTopics);
        setRedditTopics(data.redditTopics);
      } catch (error) {
        console.error('Error fetching trending topics:', error);
        // Fallback to default topics if API fails
        setTwitterTopics([
          { title: '#Technology', url: 'https://twitter.com/search?q=%23Technology' },
          { title: '#AI', url: 'https://twitter.com/search?q=%23AI' },
          { title: '#Crypto', url: 'https://twitter.com/search?q=%23Crypto' },
          { title: '#Innovation', url: 'https://twitter.com/search?q=%23Innovation' },
          { title: '#Future', url: 'https://twitter.com/search?q=%23Future' }
        ]);
        
        setRedditTopics([
          { title: 'r/technology', url: 'https://reddit.com/r/technology' },
          { title: 'r/worldnews', url: 'https://reddit.com/r/worldnews' },
          { title: 'r/science', url: 'https://reddit.com/r/science' },
          { title: 'r/futurology', url: 'https://reddit.com/r/futurology' },
          { title: 'r/space', url: 'https://reddit.com/r/space' }
        ]);
      } finally {
        setLoading(false);
      }
    };

    fetchTopics();
  }, []);

  if (loading) {
    return null;
  }

  return (
    <div className={`w-44 bg-white/10 dark:bg-gray-800/50 backdrop-blur-sm rounded-lg p-3 shadow-lg border border-gray-200 dark:border-gray-700 ${className}`}>
      <div className="space-y-4 max-h-[calc(100vh-16rem)] overflow-y-auto">
        <div>
          <div className="flex items-center gap-2 mb-2">
            <FaTwitter className="text-[#1DA1F2] h-5 w-5" />
            <span className="text-[#1DA1F2] text-sm font-medium">{t('layout.trending.twitterTitle')}</span>
          </div>
          <div className="flex flex-col gap-2">
            {twitterTopics.map((topic, index) => (
              <a
                key={index}
                href={topic.url}
                target="_blank"
                rel="noopener noreferrer"
                className="px-3 py-1 bg-[#1DA1F2]/10 hover:bg-[#1DA1F2]/20 text-[#1DA1F2] rounded-full text-sm transition-colors truncate"
              >
                {topic.title}
              </a>
            ))}
          </div>
        </div>
        <div>
          <div className="flex items-center gap-2 mb-2">
            <FaReddit className="text-[#FF4500] h-5 w-5" />
            <span className="text-[#FF4500] text-sm font-medium">{t('layout.trending.redditTitle')}</span>
          </div>
          <div className="flex flex-col gap-2">
            {redditTopics.map((topic, index) => (
              <a
                key={index}
                href={topic.url}
                target="_blank"
                rel="noopener noreferrer"
                className="px-3 py-1 bg-[#FF4500]/10 hover:bg-[#FF4500]/20 text-[#FF4500] rounded-full text-sm transition-colors truncate"
              >
                {topic.title}
              </a>
            ))}
          </div>
        </div>
      </div>
    </div>
  );
}; 