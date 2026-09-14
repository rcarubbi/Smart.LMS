"use client";

import { useCallback, useEffect, useState } from "react";
import { apiPage } from "@/lib/api";

type Params = Record<string, string | number | boolean | undefined>;

/**
 * Server-side paged list state for a GET endpoint. `params` changes reset the
 * page to 1 and refetch. Returns items, total, loading, page controls and reload.
 */
export function usePagedList<T>(endpoint: string, params: Params, pageSize = 25) {
  const [items, setItems] = useState<T[]>([]);
  const [total, setTotal] = useState(0);
  const [page, setPage] = useState(1);
  const [loading, setLoading] = useState(true);
  const enabled = endpoint.length > 0;

  const query = new URLSearchParams();
  query.set("pageSize", String(pageSize));
  for (const [key, value] of Object.entries(params)) {
    if (value !== undefined && value !== "") query.set(key, String(value));
  }
  const endpointKey = `${endpoint}?${query.toString()}`;

  const reload = useCallback(async () => {
    if (!enabled) {
      setItems([]);
      setTotal(0);
      setLoading(false);
      return;
    }
    setLoading(true);
    try {
      const paged = await apiPage<T>(`${endpointKey}&page=${page}`);
      setItems(paged.items);
      setTotal(paged.total);
    } catch {
      setItems([]);
      setTotal(0);
    } finally {
      setLoading(false);
    }
  }, [endpointKey, page, enabled]);

  useEffect(() => {
    setPage(1);
  }, [endpointKey]);

  useEffect(() => {
    reload();
  }, [reload]);

  return { items, total, loading, page, setPage, setItems, reload };
}