import defaultTranslations from '../translations/translations.json';

export type Language = 'en' | 'el';

export interface Translations {
  [key: string]: {
    [lang in Language]: string;
  };
}

class TranslationService {
  private translations: Translations = defaultTranslations;
  private initialized: boolean = false;

  /**
   * Initializes the translation service by loading translations from the JSON file
   * @returns Promise<void>
   */
  public async initialize(): Promise<void> {
    try {
      // We already have the translations from the imported JSON
      // This is kept for future dynamic loading if needed
      this.initialized = true;
    } catch (error) {
      console.error('Failed to initialize translations:', error);
      // Fallback to default translations is automatic since we imported them
    }
  }

  /**
   * Translates a key to the specified language
   * @param key The translation key
   * @param language The target language
   * @returns The translated string or the key if translation is not found
   */
  public translate(key: string, language: Language): string {
    if (!this.initialized) {
      console.warn('TranslationService not initialized, using default translations');
    }

    try {
      const translation = this.translations[key]?.[language];
      if (!translation) {
        console.warn(`Translation not found for key: ${key} in language: ${language}`);
        return key;
      }
      return translation;
    } catch (error) {
      console.error('Error translating key:', key, error);
      return key;
    }
  }

  /**
   * Returns all available translations
   * @returns The translations object
   */
  public getTranslations(): Translations {
    return this.translations;
  }

  /**
   * Checks if a translation key exists
   * @param key The translation key to check
   * @returns boolean indicating if the key exists
   */
  public hasTranslation(key: string): boolean {
    return key in this.translations;
  }

  /**
   * Gets all available keys
   * @returns Array of translation keys
   */
  public getKeys(): string[] {
    return Object.keys(this.translations);
  }
}

// Export a singleton instance
export const translationService = new TranslationService();
export default translationService; 