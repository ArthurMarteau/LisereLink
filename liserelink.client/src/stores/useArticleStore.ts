import { create } from 'zustand'
import { persist } from 'zustand/middleware'
import { idbStorage } from './idbStorage'
import type { ArticleDto } from '@/types'

interface ArticleState {
  searchResults: ArticleDto[]
  selectedArticle: ArticleDto | null
  isLoading: boolean
  setSearchResults: (results: ArticleDto[]) => void
  setSelectedArticle: (article: ArticleDto | null) => void
  clearSearch: () => void
  setLoading: (loading: boolean) => void
}

export const useArticleStore = create<ArticleState>()(
  persist(
    (set) => ({
      searchResults: [],
      selectedArticle: null,
      isLoading: false,
      setSearchResults: (searchResults) => set({ searchResults }),
      setSelectedArticle: (selectedArticle) => set({ selectedArticle }),
      clearSearch: () => set({ searchResults: [], selectedArticle: null }),
      setLoading: (isLoading) => set({ isLoading }),
    }),
    { name: 'lisere-articles', storage: idbStorage },
  ),
)
