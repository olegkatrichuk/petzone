import { useState } from 'react'
import { useTranslation } from 'react-i18next'
import Paper from '@mui/material/Paper'
import Typography from '@mui/material/Typography'
import Box from '@mui/material/Box'
import TextField from '@mui/material/TextField'
import Select from '@mui/material/Select'
import MenuItem from '@mui/material/MenuItem'
import FormControl from '@mui/material/FormControl'
import InputLabel from '@mui/material/InputLabel'
import Button from '@mui/material/Button'
import Divider from '@mui/material/Divider'
import FilterAltIcon from '@mui/icons-material/FilterAlt'
import { useGetSpeciesQuery, useGetBreedsQuery, skipToken } from '../../services/speciesApi'
import type { PetFilters } from '../../types/pet'

const CORAL = '#FF6B6B'

type HealthOption = '' | 'vaccinated' | 'castrated' | 'both'

function healthToFilters(h: HealthOption): Pick<PetFilters, 'isVaccinated' | 'isCastrated'> {
  if (h === 'vaccinated') return { isVaccinated: true, isCastrated: undefined }
  if (h === 'castrated') return { isCastrated: true, isVaccinated: undefined }
  if (h === 'both') return { isVaccinated: true, isCastrated: true }
  return { isVaccinated: undefined, isCastrated: undefined }
}

interface LocalState {
  nickname: string
  color: string
  city: string
  health: HealthOption
  status: string
  speciesId: string
  breedId: string
  minAge: string
  maxAge: string
  minWeight: string
  maxWeight: string
  sortBy: string
  sortDescending: string
}

const EMPTY: LocalState = {
  nickname: '', color: '', city: '', health: '', status: '',
  speciesId: '', breedId: '', minAge: '', maxAge: '',
  minWeight: '', maxWeight: '', sortBy: '', sortDescending: 'false',
}

function filtersToLocal(f: PetFilters): LocalState {
  let health: HealthOption = ''
  if (f.isVaccinated && f.isCastrated) health = 'both'
  else if (f.isVaccinated) health = 'vaccinated'
  else if (f.isCastrated) health = 'castrated'
  return {
    nickname: f.nickname ?? '',
    color: f.color ?? '',
    city: f.city ?? '',
    health,
    status: f.status !== undefined ? String(f.status) : '',
    speciesId: f.speciesId ?? '',
    breedId: f.breedId ?? '',
    minAge: f.minAge !== undefined ? String(f.minAge) : '',
    maxAge: f.maxAge !== undefined ? String(f.maxAge) : '',
    minWeight: f.minWeight !== undefined ? String(f.minWeight) : '',
    maxWeight: f.maxWeight !== undefined ? String(f.maxWeight) : '',
    sortBy: f.sortBy === 'dateOfBirth' ? 'age' : (f.sortBy ?? ''),
    sortDescending: f.sortDescending ? 'true' : 'false',
  }
}

interface Props {
  initialFilters: PetFilters
  onApply: (filters: PetFilters) => void
}

export default function PetFiltersPanel({ initialFilters, onApply }: Props) {
  const { t, i18n } = useTranslation()
  const [local, setLocal] = useState<LocalState>(() => filtersToLocal(initialFilters))

  const set = <K extends keyof LocalState>(key: K, value: LocalState[K]) =>
    setLocal((prev) => ({ ...prev, [key]: value, ...(key === 'speciesId' ? { breedId: '' } : {}) }))

  // RTK Query — species list (fetched once, cached globally)
  const locale = i18n.language?.slice(0, 2) || 'uk'
  const { data: species = [] } = useGetSpeciesQuery(locale)

  // RTK Query — breeds for the selected species
  // skipToken prevents the request when no species is selected
  const { data: breeds = [] } = useGetBreedsQuery(
    local.speciesId ? { speciesId: local.speciesId, locale } : skipToken,
  )

  const buildFilters = (overrides: Partial<LocalState> = {}): PetFilters => {
    const merged = { ...local, ...overrides }
    return {
      ...initialFilters,
      page: 1,
      nickname: merged.nickname || undefined,
      color: merged.color || undefined,
      city: merged.city || undefined,
      status: merged.status !== '' ? Number(merged.status) : undefined,
      speciesId: merged.speciesId || undefined,
      breedId: merged.breedId || undefined,
      minAge: merged.minAge !== '' ? Number(merged.minAge) : undefined,
      maxAge: merged.maxAge !== '' ? Number(merged.maxAge) : undefined,
      minWeight: merged.minWeight !== '' ? Number(merged.minWeight) : undefined,
      maxWeight: merged.maxWeight !== '' ? Number(merged.maxWeight) : undefined,
      sortBy: merged.sortBy || undefined,
      sortDescending: merged.sortDescending === 'true' ? true : undefined,
      ...healthToFilters(merged.health as HealthOption),
    }
  }

  // Species and breed apply immediately on change (no Apply click needed)
  const handleSpeciesChange = (value: string) => {
    set('speciesId', value)
    onApply(buildFilters({ speciesId: value, breedId: '' }))
  }

  const handleBreedChange = (value: string) => {
    set('breedId', value)
    onApply(buildFilters({ breedId: value }))
  }

  const handleApply = () => onApply(buildFilters())

  const handleReset = () => {
    setLocal(EMPTY)
    onApply({ page: 1, pageSize: initialFilters.pageSize })
  }

  return (
    <Paper
      elevation={0}
      sx={{ border: '1px solid #E5E7EB', borderRadius: 3, p: 3, position: 'sticky', top: 16 }}
    >
      <Box sx={{ display: 'flex', alignItems: 'center', gap: 1, mb: 2 }}>
        <FilterAltIcon sx={{ color: CORAL }} />
        <Typography variant="h6" fontWeight="bold">{t('pets.filters')}</Typography>
      </Box>
      <Divider sx={{ mb: 3 }} />

      <Box sx={{ display: 'flex', flexDirection: 'column', gap: 2.5 }}>

        {/* Nickname */}
        <TextField
          label={t('pets.searchByName')}
          size="small" fullWidth
          value={local.nickname}
          onChange={(e) => set('nickname', e.target.value)}
        />

        {/* Species */}
        <FormControl size="small" fullWidth>
          <InputLabel>{t('pets.species')}</InputLabel>
          <Select label={t('pets.species')} value={local.speciesId} onChange={(e) => handleSpeciesChange(e.target.value)}>
            <MenuItem value="">{t('pets.any')}</MenuItem>
            {species.map((s) => (
              <MenuItem key={s.id} value={s.id}>{s.name}</MenuItem>
            ))}
          </Select>
        </FormControl>

        {/* Breed — active only when species selected */}
        <FormControl size="small" fullWidth disabled={!local.speciesId}>
          <InputLabel>{t('pets.breed')}</InputLabel>
          <Select label={t('pets.breed')} value={local.breedId} onChange={(e) => handleBreedChange(e.target.value)}>
            <MenuItem value="">{t('pets.any')}</MenuItem>
            {breeds.map((b) => (
              <MenuItem key={b.id} value={b.id}>{b.name}</MenuItem>
            ))}
          </Select>
        </FormControl>

        {/* Color */}
        <FormControl size="small" fullWidth>
          <InputLabel>{t('pets.color.label')}</InputLabel>
          <Select label={t('pets.color.label')} value={local.color} onChange={(e) => set('color', e.target.value)}>
            {(['', 'black', 'white', 'gray', 'orange', 'mixed'] as const).map((v) => (
              <MenuItem key={v} value={v}>{t(`pets.color.${v === '' ? 'any' : v}`)}</MenuItem>
            ))}
          </Select>
        </FormControl>

        {/* City */}
        <TextField
          label={t('pets.location.label')}
          size="small" fullWidth
          value={local.city}
          onChange={(e) => set('city', e.target.value)}
        />

        {/* Age range */}
        <Typography variant="caption" color="text.secondary" sx={{ mb: -1.5 }}>
          {t('pets.ageRange')}
        </Typography>
        <Box sx={{ display: 'flex', gap: 1 }}>
          <TextField
            label={t('pets.minAge')} size="small" type="number" fullWidth
            value={local.minAge}
            onChange={(e) => set('minAge', e.target.value)}
            inputProps={{ min: 0 }}
          />
          <TextField
            label={t('pets.maxAge')} size="small" type="number" fullWidth
            value={local.maxAge}
            onChange={(e) => set('maxAge', e.target.value)}
            inputProps={{ min: 0 }}
          />
        </Box>

        {/* Weight range */}
        <Typography variant="caption" color="text.secondary" sx={{ mb: -1.5 }}>
          {t('pets.weightRange')}
        </Typography>
        <Box sx={{ display: 'flex', gap: 1 }}>
          <TextField
            label={t('pets.minWeight')} size="small" type="number" fullWidth
            value={local.minWeight}
            onChange={(e) => set('minWeight', e.target.value)}
            inputProps={{ min: 0, step: 0.1 }}
          />
          <TextField
            label={t('pets.maxWeight')} size="small" type="number" fullWidth
            value={local.maxWeight}
            onChange={(e) => set('maxWeight', e.target.value)}
            inputProps={{ min: 0, step: 0.1 }}
          />
        </Box>

        {/* Health */}
        <FormControl size="small" fullWidth>
          <InputLabel>{t('pets.health.label')}</InputLabel>
          <Select label={t('pets.health.label')} value={local.health} onChange={(e) => set('health', e.target.value as HealthOption)}>
            <MenuItem value="">{t('pets.health.any')}</MenuItem>
            <MenuItem value="vaccinated">{t('pets.health.vaccinated')}</MenuItem>
            <MenuItem value="castrated">{t('pets.health.castrated')}</MenuItem>
            <MenuItem value="both">{t('pets.health.both')}</MenuItem>
          </Select>
        </FormControl>

        {/* Status */}
        <FormControl size="small" fullWidth>
          <InputLabel>{t('pets.statusLabel')}</InputLabel>
          <Select label={t('pets.statusLabel')} value={local.status} onChange={(e) => set('status', e.target.value)}>
            <MenuItem value="">{t('pets.any')}</MenuItem>
            <MenuItem value="0">{t('pets.status.needsHelp')}</MenuItem>
            <MenuItem value="1">{t('pets.status.lookingForHome')}</MenuItem>
            <MenuItem value="2">{t('pets.status.foundHome')}</MenuItem>
          </Select>
        </FormControl>

        {/* Sort */}
        <FormControl size="small" fullWidth>
          <InputLabel>{t('pets.sortBy')}</InputLabel>
          <Select label={t('pets.sortBy')} value={local.sortBy} onChange={(e) => set('sortBy', e.target.value)}>
            <MenuItem value="">{t('pets.any')}</MenuItem>
            <MenuItem value="nickname">{t('pets.sortByName')}</MenuItem>
            <MenuItem value="age">{t('pets.sortByAge')}</MenuItem>
            <MenuItem value="city">{t('pets.sortByCity')}</MenuItem>
            <MenuItem value="weight">{t('pets.sortByWeight')}</MenuItem>
          </Select>
        </FormControl>

        <FormControl size="small" fullWidth>
          <InputLabel>{t('pets.sortOrder')}</InputLabel>
          <Select label={t('pets.sortOrder')} value={local.sortDescending} onChange={(e) => set('sortDescending', e.target.value)}>
            <MenuItem value="false">{t('pets.sortAsc')}</MenuItem>
            <MenuItem value="true">{t('pets.sortDesc')}</MenuItem>
          </Select>
        </FormControl>

        <Button
          variant="contained" fullWidth onClick={handleApply}
          sx={{ bgcolor: CORAL, '&:hover': { bgcolor: '#e55555' }, borderRadius: 2, textTransform: 'none', fontWeight: 700, py: 1.2, mt: 1 }}
        >
          {t('pets.apply')}
        </Button>

        <Button variant="text" fullWidth onClick={handleReset} sx={{ textTransform: 'none', color: '#9CA3AF' }}>
          {t('pets.reset')}
        </Button>
      </Box>
    </Paper>
  )
}
