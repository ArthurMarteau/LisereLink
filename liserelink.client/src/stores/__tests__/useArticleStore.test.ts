import { describe, it, expect, beforeEach, vi } from 'vitest';
import { ClothingFamily, Size } from '@/constants/enums';
import type { ArticleDto } from '@/types';

vi.mock('idb-keyval', () => ({
  get: vi.fn().mockResolvedValue(null),
  set: vi.fn().mockResolvedValue(undefined),
  del: vi.fn().mockResolvedValue(undefined),
}));

const { useArticleStore } = await import('../useArticleStore');

function makeArticle(overrides: Partial<ArticleDto> = {}): ArticleDto {
  return {
    id: crypto.randomUUID(),
    barcode: '1234567890123',
    family: ClothingFamily.TSH,
    name: 'T-shirt basique',
    colorOrPrint: 'Blanc',
    availableSizes: [Size.S, Size.M, Size.L],
    ...overrides,
  };
}

beforeEach(() => {
  useArticleStore.setState({ searchResults: [], selectedArticle: null, isLoading: false });
});

describe('useArticleStore — initial state', () => {
  it('searchResults is an empty array', () => {
    expect(useArticleStore.getState().searchResults).toEqual([]);
  });

  it('selectedArticle is null', () => {
    expect(useArticleStore.getState().selectedArticle).toBeNull();
  });

  it('isLoading is false', () => {
    expect(useArticleStore.getState().isLoading).toBe(false);
  });
});

describe('useArticleStore — setSearchResults', () => {
  it('replaces the search results', () => {
    const a1 = makeArticle();
    const a2 = makeArticle();
    useArticleStore.getState().setSearchResults([a1, a2]);
    expect(useArticleStore.getState().searchResults).toEqual([a1, a2]);
  });

  it('clears results when given an empty array', () => {
    useArticleStore.getState().setSearchResults([makeArticle()]);
    useArticleStore.getState().setSearchResults([]);
    expect(useArticleStore.getState().searchResults).toEqual([]);
  });
});

describe('useArticleStore — setSelectedArticle', () => {
  it('stores the selected article', () => {
    const article = makeArticle();
    useArticleStore.getState().setSelectedArticle(article);
    expect(useArticleStore.getState().selectedArticle).toEqual(article);
  });

  it('clears the selection when null is passed', () => {
    useArticleStore.getState().setSelectedArticle(makeArticle());
    useArticleStore.getState().setSelectedArticle(null);
    expect(useArticleStore.getState().selectedArticle).toBeNull();
  });

  it('does not modify searchResults', () => {
    const results = [makeArticle(), makeArticle()];
    useArticleStore.getState().setSearchResults(results);
    useArticleStore.getState().setSelectedArticle(makeArticle());
    expect(useArticleStore.getState().searchResults).toEqual(results);
  });
});

describe('useArticleStore — clearSearch', () => {
  it('resets searchResults to an empty array', () => {
    useArticleStore.getState().setSearchResults([makeArticle()]);
    useArticleStore.getState().clearSearch();
    expect(useArticleStore.getState().searchResults).toEqual([]);
  });

  it('resets selectedArticle to null', () => {
    useArticleStore.getState().setSelectedArticle(makeArticle());
    useArticleStore.getState().clearSearch();
    expect(useArticleStore.getState().selectedArticle).toBeNull();
  });

  it('does not modify isLoading', () => {
    useArticleStore.getState().setLoading(true);
    useArticleStore.getState().clearSearch();
    expect(useArticleStore.getState().isLoading).toBe(true);
  });
});

describe('useArticleStore — setLoading', () => {
  it('setLoading(true) sets isLoading to true', () => {
    useArticleStore.getState().setLoading(true);
    expect(useArticleStore.getState().isLoading).toBe(true);
  });

  it('setLoading(false) sets isLoading to false', () => {
    useArticleStore.getState().setLoading(true);
    useArticleStore.getState().setLoading(false);
    expect(useArticleStore.getState().isLoading).toBe(false);
  });
});
