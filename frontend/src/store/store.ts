import { configureStore } from '@reduxjs/toolkit'
import { petsApi } from '../services/petsApi'
import { speciesApi } from '../services/speciesApi'
import { volunteersApi } from '../services/volunteersApi'
import { accountsApi } from '../services/accountsApi'
import { volunteerRequestsApi } from '../services/volunteerRequestsApi'
import { adminApi } from '../services/adminApi'
import { discussionApi } from '../services/discussionApi'
import { listingsApi } from '../services/listingsApi'
import { newsApi } from '../services/newsApi'
import { adoptionApi } from '../services/adoptionApi'

export const store = configureStore({
  reducer: {
    [petsApi.reducerPath]: petsApi.reducer,
    [speciesApi.reducerPath]: speciesApi.reducer,
    [volunteersApi.reducerPath]: volunteersApi.reducer,
    [accountsApi.reducerPath]: accountsApi.reducer,
    [volunteerRequestsApi.reducerPath]: volunteerRequestsApi.reducer,
    [adminApi.reducerPath]: adminApi.reducer,
    [discussionApi.reducerPath]: discussionApi.reducer,
    [listingsApi.reducerPath]: listingsApi.reducer,
    [newsApi.reducerPath]: newsApi.reducer,
    [adoptionApi.reducerPath]: adoptionApi.reducer,
  },
  middleware: (getDefaultMiddleware) =>
    getDefaultMiddleware().concat(
      petsApi.middleware,
      speciesApi.middleware,
      volunteersApi.middleware,
      accountsApi.middleware,
      volunteerRequestsApi.middleware,
      adminApi.middleware,
      discussionApi.middleware,
      listingsApi.middleware,
      newsApi.middleware,
      adoptionApi.middleware,
    ),
})

export type RootState = ReturnType<typeof store.getState>
export type AppDispatch = typeof store.dispatch
