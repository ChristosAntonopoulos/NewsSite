import { useUserPreferences } from '../../../hooks/useUserPreferences';

export function UserPreferences() {
  // This hook will handle syncing user preferences
  useUserPreferences();
  
  // This is a utility component that doesn't render anything
  return null;
} 