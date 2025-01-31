import { useState } from 'react';
import { useNavigate, Link } from 'react-router-dom';
import { authApi } from '../services/authApi';
import { useAuth } from '../contexts/AuthContext';
import { useLanguage } from '../contexts/LanguageContext';
import { useTheme } from '../contexts/ThemeContext';
import { FaGoogle, FaReddit } from 'react-icons/fa';

export function LoginPage() {
  const navigate = useNavigate();
  const { setUser, setToken } = useAuth();
  const { t, language, setLanguage } = useLanguage();
  const { theme, toggleTheme } = useTheme();
  
  const [formData, setFormData] = useState({
    email: '',
    password: ''
  });
  const [error, setError] = useState('');
  const [loading, setLoading] = useState(false);

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setError('');
    setLoading(true);

    try {
      const response = await authApi.login(formData);
      setUser(response.user);
      setToken(response.token);
      navigate('/');
    } catch (err) {
      setError(t('auth.login.errors.DEFAULT'));
    } finally {
      setLoading(false);
    }
  };

  const handleGoogleLogin = async () => {
    // TODO: Implement Google login
    window.alert('Google login not implemented yet');
  };

  const handleRedditLogin = async () => {
    // TODO: Implement Reddit login
    window.alert('Reddit login not implemented yet');
  };

  return (
    <div className="min-h-screen flex items-center justify-center bg-gray-100 dark:bg-gray-900">
      <div className="max-w-md w-full space-y-8 p-8 bg-white dark:bg-gray-800 rounded-xl shadow-lg">
        <div className="text-center">
          <h2 className="text-3xl font-bold text-gray-900 dark:text-white">
            {t('auth.login.title')}
          </h2>
          <div className="mt-2 flex justify-center space-x-4">
            <select
              value={language}
              onChange={(e) => setLanguage(e.target.value as 'en' | 'el')}
              className="text-sm text-gray-500 dark:text-gray-400 bg-transparent"
            >
              <option value="en">English</option>
              <option value="el">Ελληνικά</option>
            </select>
            <button
              onClick={toggleTheme}
              className="text-gray-500 dark:text-gray-400"
            >
              {theme === 'dark' ? '☀️' : '🌙'}
            </button>
          </div>
        </div>

        {error && (
          <div className="bg-red-100 dark:bg-red-900 text-red-700 dark:text-red-200 p-3 rounded">
            {error}
          </div>
        )}

        <div className="flex flex-col space-y-4">
          <button
            onClick={handleGoogleLogin}
            className="flex items-center justify-center space-x-2 w-full py-3 px-4 bg-white hover:bg-gray-50 text-gray-900 border border-gray-300 rounded-md transition duration-200"
          >
            <FaGoogle className="text-red-600" />
            <span>{t('auth.login.googleButton')}</span>
          </button>

          <button
            onClick={handleRedditLogin}
            className="flex items-center justify-center space-x-2 w-full py-3 px-4 bg-[#FF4500] hover:bg-[#FF5700] text-white rounded-md transition duration-200"
          >
            <FaReddit />
            <span>{t('auth.login.redditButton')}</span>
          </button>

          <div className="relative">
            <div className="absolute inset-0 flex items-center">
              <div className="w-full border-t border-gray-300 dark:border-gray-600"></div>
            </div>
            <div className="relative flex justify-center text-sm">
              <span className="px-2 bg-white dark:bg-gray-800 text-gray-500 dark:text-gray-400">
                {t('auth.login.orDivider')}
              </span>
            </div>
          </div>
        </div>

        <form className="mt-8 space-y-6" onSubmit={handleSubmit}>
          <div>
            <label className="text-gray-700 dark:text-gray-300">
              {t('auth.login.email')}
            </label>
            <input
              type="email"
              value={formData.email}
              onChange={(e) => setFormData({ ...formData, email: e.target.value })}
              className="w-full mt-2 p-3 border border-gray-300 dark:border-gray-600 rounded-md
                       bg-white dark:bg-gray-700 text-gray-900 dark:text-white"
            />
          </div>

          <div>
            <label className="text-gray-700 dark:text-gray-300">
              {t('auth.login.password')}
            </label>
            <input
              type="password"
              value={formData.password}
              onChange={(e) => setFormData({ ...formData, password: e.target.value })}
              className="w-full mt-2 p-3 border border-gray-300 dark:border-gray-600 rounded-md
                       bg-white dark:bg-gray-700 text-gray-900 dark:text-white"
            />
          </div>

          <button
            type="submit"
            disabled={loading}
            className="w-full py-3 px-4 bg-teal-600 hover:bg-teal-700 text-white rounded-md
                     transition duration-200 disabled:opacity-50"
          >
            {loading ? '...' : t('auth.login.submit')}
          </button>
        </form>

        <div className="text-center mt-4">
          <Link
            to="/register"
            className="text-teal-600 hover:text-teal-700 dark:text-teal-400 dark:hover:text-teal-300"
          >
            {t('auth.login.registerLink')}
          </Link>
        </div>
      </div>
    </div>
  );
} 