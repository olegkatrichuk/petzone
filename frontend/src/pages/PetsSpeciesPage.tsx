import { useMemo, useEffect, useState } from 'react'
import { useParams, Navigate } from 'react-router-dom'
import { useTranslation } from 'react-i18next'
import Box from '@mui/material/Box'
import Container from '@mui/material/Container'
import Typography from '@mui/material/Typography'
import Chip from '@mui/material/Chip'
import Divider from '@mui/material/Divider'
import PageMeta from '../components/meta/PageMeta'
import AppBreadcrumbs from '../components/ui/AppBreadcrumbs'
import PetsList from '../components/pets/PetsList'
import { useGetSpeciesQuery } from '../services/speciesApi'
import { useGetPetsQuery } from '../services/petsApi'
import { PetStatus, type Pet } from '../types/pet'
import { useLangNavigate } from '../hooks/useLangNavigate'

const CORAL = '#FF6B6B'
const SITE_URL = 'https://getpetzone.com'

interface SpeciesConfig {
  ukName: string
  emoji: string
  title: string
  h1: string
  description: (cityLocative?: string) => string
  seoText: string
  relatedSlugs: string[]
}

const SPECIES_CONFIG: Record<string, SpeciesConfig> = {
  dogs: {
    ukName: 'Собака',
    emoji: '🐕',
    title: 'Собаки для усиновлення',
    h1: 'Собаки шукають дім',
    description: (city) =>
      city
        ? `Усиновлення собак у ${city} безкоштовно — оголошення від волонтерів. Лабрадори, хаскі, вівчарки та безпородні пси чекають на господаря.`
        : 'Усиновлення собак безкоштовно — сотні оголошень від волонтерів по всій Україні. Лабрадори, хаскі, вівчарки, шпіци та безпородні пси чекають на нового господаря.',
    seoText:
      'PetZone — найбільший безкоштовний каталог собак для усиновлення в Україні. Волонтери з Києва, Харкова, Львова, Одеси та інших міст публікують оголошення про собак, яким потрібен новий дім. Кожна тварина вакцинована та перевірена. Обирайте за породою, містом, віком і приходьте знайомитись.',
    relatedSlugs: ['cats', 'rabbits'],
  },
  cats: {
    ukName: 'Кішка',
    emoji: '🐈',
    title: 'Коти для усиновлення',
    h1: 'Коти шукають дім',
    description: (city) =>
      city
        ? `Усиновлення котів у ${city} безкоштовно — оголошення від волонтерів. Британські, мейн-куни, сфінкси та домашні коти чекають на господаря.`
        : 'Усиновлення котів безкоштовно — британські короткошерсті, мейн-куни, сфінкси та домашні коти від волонтерів з Києва, Харкова, Львова та інших міст України.',
    seoText:
      'PetZone — безкоштовний каталог котів для усиновлення. Волонтери з усієї України публікують оголошення про котів, яким потрібен дім. Знайдіть свого кота серед сотень оголошень — фільтруйте за породою, віком і містом.',
    relatedSlugs: ['dogs', 'rabbits'],
  },
  rabbits: {
    ukName: 'Кролик',
    emoji: '🐇',
    title: 'Кролики для усиновлення',
    h1: 'Кролики шукають дім',
    description: (city) =>
      city
        ? `Усиновлення кроликів у ${city} безкоштовно — вислоухі, карликові, ангорські та декоративні кролики від волонтерів.`
        : 'Усиновлення кроликів безкоштовно — вислоухі, карликові, ангорські та декоративні кролики від волонтерів по всій Україні.',
    seoText:
      'Декоративні кролики — чудові домашні улюбленці. На PetZone ви знайдете кроликів для усиновлення від перевірених волонтерів України. Усі тварини здорові та готові до нового дому.',
    relatedSlugs: ['dogs', 'cats'],
  },
  parrots: {
    ukName: 'Папуга',
    emoji: '🦜',
    title: 'Папуги для усиновлення',
    h1: 'Папуги шукають дім',
    description: (city) =>
      city
        ? `Усиновлення папуг у ${city} безкоштовно — хвилясті, жако, корели та інші папуги від волонтерів.`
        : 'Усиновлення папуг безкоштовно — хвилясті папужки, жако, корели, какаду від волонтерів України.',
    seoText:
      'Папуги — розумні та відданні улюбленці. На PetZone волонтери публікують оголошення про папуг, яким потрібен дім. Знайдіть свого пернатого друга безкоштовно.',
    relatedSlugs: ['dogs', 'cats'],
  },
}

interface CityConfig {
  name: string
  locative: string
  slug: string
}

const CITIES: CityConfig[] = [
  { name: 'Київ', locative: 'Києві', slug: 'kyiv' },
  { name: 'Харків', locative: 'Харкові', slug: 'kharkiv' },
  { name: 'Львів', locative: 'Львові', slug: 'lviv' },
  { name: 'Одеса', locative: 'Одесі', slug: 'odesa' },
  { name: 'Дніпро', locative: 'Дніпрі', slug: 'dnipro' },
  { name: 'Запоріжжя', locative: 'Запоріжжі', slug: 'zaporizhzhia' },
]

const CITY_BY_SLUG: Record<string, CityConfig> = Object.fromEntries(CITIES.map((c) => [c.slug, c]))

const PAGE_SIZE = 12

export default function PetsSpeciesPage() {
  const { speciesSlug = '', citySlug } = useParams<{ speciesSlug: string; citySlug?: string }>()
  const { t } = useTranslation()
  const navigate = useLangNavigate()
  const [page, setPage] = useState(1)
  const [accumulated, setAccumulated] = useState<Pet[]>([])

  const config = SPECIES_CONFIG[speciesSlug]
  const cityConfig = citySlug ? CITY_BY_SLUG[citySlug] : undefined

  const { data: speciesList } = useGetSpeciesQuery('uk')
  const speciesId = useMemo(
    () => speciesList?.find((s) => s.name === config?.ukName)?.id,
    [speciesList, config],
  )

  const { data, isLoading, isFetching } = useGetPetsQuery(
    { page, pageSize: PAGE_SIZE, speciesId, city: cityConfig?.name, status: PetStatus.LookingForHome },
    { skip: !speciesId },
  )

  useEffect(() => {
    if (!data?.items) return
    if (page === 1) setAccumulated(data.items)
    else setAccumulated((prev) => [...prev, ...data.items])
  // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [data])

  // Reset on slug change
  useEffect(() => {
    setPage(1)
    setAccumulated([])
  }, [speciesSlug, citySlug])

  if (!config) return <Navigate to="/pets" replace />
  if (citySlug && !cityConfig) return <Navigate to={`/pets/${speciesSlug}`} replace />

  const pageTitle = cityConfig ? `${config.title} у ${cityConfig.locative}` : config.title
  const metaPath = `/pets/${speciesSlug}${citySlug ? '/' + citySlug : ''}`
  const hasMore = accumulated.length < (data?.totalCount ?? 0)

  return (
    <Box sx={{ bgcolor: 'background.default', minHeight: '100%', py: 4 }}>
      <PageMeta title={pageTitle} description={config.description(cityConfig?.locative)} path={metaPath} />
      {accumulated.length > 0 && (
        <script
          type="application/ld+json"
          dangerouslySetInnerHTML={{
            __html: JSON.stringify({
              '@context': 'https://schema.org',
              '@type': 'ItemList',
              name: pageTitle,
              numberOfItems: data?.totalCount ?? accumulated.length,
              itemListElement: accumulated.slice(0, 10).map((pet, i) => ({
                '@type': 'ListItem',
                position: i + 1,
                name: pet.nickname,
                url: `${SITE_URL}/uk/pets/${pet.id}`,
              })),
            }),
          }}
        />
      )}
      <script
        type="application/ld+json"
        dangerouslySetInnerHTML={{
          __html: JSON.stringify({
            '@context': 'https://schema.org',
            '@type': 'BreadcrumbList',
            itemListElement: [
              { '@type': 'ListItem', position: 1, name: t('nav.home'), item: `${SITE_URL}/uk` },
              { '@type': 'ListItem', position: 2, name: t('pets.pageTitle'), item: `${SITE_URL}/uk/pets` },
              { '@type': 'ListItem', position: 3, name: config.h1, item: `${SITE_URL}/uk/pets/${speciesSlug}` },
              ...(cityConfig ? [{ '@type': 'ListItem', position: 4, name: cityConfig.name }] : []),
            ],
          }),
        }}
      />

      <Container maxWidth="xl">
        <AppBreadcrumbs
          items={[
            { label: t('nav.home'), path: '/' },
            { label: t('pets.pageTitle'), path: '/pets' },
            cityConfig
              ? { label: config.h1, path: `/pets/${speciesSlug}` }
              : { label: config.h1 },
            ...(cityConfig ? [{ label: cityConfig.name }] : []),
          ]}
        />

        {/* Heading */}
        <Box sx={{ mb: 4 }}>
          <Typography
            variant="h1"
            fontSize={{ xs: '1.6rem', sm: '2rem' }}
            fontWeight="bold"
            sx={{ color: '#1F2937', mb: 1 }}
          >
            {config.emoji} {pageTitle}
          </Typography>
          <Typography variant="body1" color="text.secondary" sx={{ maxWidth: 700 }}>
            {config.description(cityConfig?.locative)}
          </Typography>
        </Box>

        {/* City filter chips — only on root species page */}
        {!citySlug && (
          <Box sx={{ mb: 4 }}>
            <Typography variant="subtitle2" fontWeight={600} sx={{ mb: 1.5, color: '#6B7280' }}>
              За містом:
            </Typography>
            <Box sx={{ display: 'flex', gap: 1, flexWrap: 'wrap' }}>
              {CITIES.map((city) => (
                <Chip
                  key={city.slug}
                  label={city.name}
                  clickable
                  onClick={() => navigate(`/pets/${speciesSlug}/${city.slug}`)}
                  variant="outlined"
                  sx={{ borderColor: CORAL, color: CORAL, '&:hover': { bgcolor: '#FFF5F5' } }}
                />
              ))}
            </Box>
          </Box>
        )}

        {/* Pet grid */}
        <PetsList pets={accumulated} isLoading={isLoading} isFetching={isFetching} />

        {/* Load more */}
        {hasMore && !isLoading && (
          <Box sx={{ textAlign: 'center', mt: 4 }}>
            <Chip
              label={isFetching ? t('pets.loading') : t('pets.loadMore')}
              clickable={!isFetching}
              onClick={() => !isFetching && setPage((p) => p + 1)}
              sx={{
                px: 3,
                py: 2.5,
                fontSize: '0.95rem',
                bgcolor: CORAL,
                color: '#fff',
                '&:hover': { bgcolor: '#e55555' },
              }}
            />
          </Box>
        )}

        {/* Related links */}
        <Divider sx={{ my: 5 }} />
        <Box sx={{ mb: 4 }}>
          <Typography variant="subtitle1" fontWeight={600} sx={{ mb: 2 }}>
            Також шукають:
          </Typography>
          <Box sx={{ display: 'flex', gap: 1.5, flexWrap: 'wrap' }}>
            {config.relatedSlugs.map((slug) => {
              const rel = SPECIES_CONFIG[slug]
              return (
                <Chip
                  key={slug}
                  label={`${rel.emoji} ${rel.title}`}
                  clickable
                  onClick={() => navigate(`/pets/${slug}`)}
                  variant="outlined"
                  sx={{ borderColor: '#D1D5DB', color: '#374151', '&:hover': { bgcolor: '#F9FAFB' } }}
                />
              )
            })}
            <Chip
              label="Всі тварини"
              clickable
              onClick={() => navigate('/pets')}
              variant="outlined"
              sx={{ borderColor: '#D1D5DB', color: '#374151', '&:hover': { bgcolor: '#F9FAFB' } }}
            />
          </Box>
        </Box>

        {/* SEO text */}
        <Box sx={{ bgcolor: '#F9FAFB', borderRadius: 2, p: 3, mb: 4 }}>
          <Typography variant="body2" color="text.secondary" sx={{ lineHeight: 1.8 }}>
            {config.seoText}
          </Typography>
        </Box>
      </Container>
    </Box>
  )
}