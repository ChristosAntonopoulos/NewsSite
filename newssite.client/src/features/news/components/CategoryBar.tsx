import React from 'react';
import { Category } from '../../../types/Article';
import { useLanguage } from '../../../contexts/LanguageContext';

interface CategoryBarProps {
  categories: Category[];
  selectedCategory: string | null;
  onCategorySelect: (categoryId: string) => void;
}

const CategoryBar: React.FC<CategoryBarProps> = ({
  categories,
  selectedCategory,
  onCategorySelect,
}) => {
  const { language, t } = useLanguage();

  return (
    <div className="category-bar">
      <div className="categories-wrapper">
        <button
          className={`category-button ${!selectedCategory ? 'active' : ''}`}
          onClick={() => onCategorySelect('')}
        >
          {t('news.allCategories')}
        </button>
        {categories.map((category) => (
          <button
            key={category.id}
            className={`category-button ${selectedCategory === category.id ? 'active' : ''}`}
            onClick={() => onCategorySelect(category.id)}
          >
            {language === 'el' ? category.name.el : category.name.en}
          </button>
        ))}
      </div>
    </div>
  );
};

export default CategoryBar; 