import { useActionState } from 'react'
import { useNavigate } from 'react-router-dom'
import { toast } from 'sonner'
import apiClient from '@/services/apiClient'
import { useAuthStore } from '@/stores/authStore'
import type { ProblemDetails, UserDto } from '@/types'

interface LoginResponse {
  token: string
  user: UserDto
}

type LoginErrors = {
  email?: string
  password?: string
}

const EMAIL_REGEX = /^[^\s@]+@[^\s@]+\.[^\s@]+$/

export default function LoginPage() {
  const navigate = useNavigate()

  const [errors, formAction, isPending] = useActionState<LoginErrors, FormData>(
    async (_prev, formData) => {
      const emailEntry = formData.get('email')
      const passwordEntry = formData.get('password')
      const email = typeof emailEntry === 'string' ? emailEntry.trim() : ''
      const password = typeof passwordEntry === 'string' ? passwordEntry : ''

      // Client-side validation
      const fieldErrors: LoginErrors = {}
      if (!email) fieldErrors.email = 'L\'adresse e-mail est requise.'
      else if (!EMAIL_REGEX.test(email)) fieldErrors.email = 'Format d\'e-mail invalide.'
      if (!password) fieldErrors.password = 'Le mot de passe est requis.'

      if (Object.keys(fieldErrors).length > 0) return fieldErrors

      try {
        const { data } = await apiClient.post<LoginResponse>('/auth/login', {
          email,
          password,
        })
        useAuthStore.getState().setToken(data.token)
        useAuthStore.getState().setUser(data.user)
        navigate('/store-selection')
        return {}
      } catch (err: unknown) {
        if (
          err !== null &&
          typeof err === 'object' &&
          'detail' in err &&
          typeof (err as ProblemDetails).detail === 'string'
        ) {
          toast.error((err as ProblemDetails).detail)
        } else {
          toast.error('Une erreur est survenue. Veuillez réessayer.')
        }
        return {}
      }
    },
    {},
  )

  return (
    <div className="min-h-screen bg-[#121212] flex flex-col">
      {/* Dark header */}
      <div className="flex-1 flex flex-col items-center justify-center px-6 pb-10">
        <h1 className="font-['Libre_Baskerville'] text-white text-[40px] tracking-[4px] mb-2">
          LISERE
        </h1>
        <p className="font-[Oswald] text-[#969696] text-[11px] tracking-[3px] uppercase">
          Gestion des demandes
        </p>
      </div>

      {/* White form card — rounded top corners */}
      <div className="bg-white rounded-t-3xl px-6 pt-8 pb-10">
        <form action={formAction} noValidate>
          {/* Email field */}
          <div className="mb-6">
            <label
              htmlFor="email"
              className="block font-[Oswald] text-[11px] tracking-[2px] uppercase text-[#969696] mb-2"
            >
              Adresse e-mail
            </label>
            <input
              id="email"
              name="email"
              type="email"
              autoComplete="email"
              inputMode="email"
              className="w-full bg-transparent border-0 border-b border-[#e1e1e1] py-2 font-['Libre_Baskerville'] text-[15px] text-[#121212] outline-none focus:border-[#121212] transition-colors"
            />
            {errors.email && (
              <p className="mt-1 font-[Oswald] text-[11px] tracking-[1px] text-[#e51940]">
                {errors.email}
              </p>
            )}
          </div>

          {/* Password field */}
          <div className="mb-8">
            <label
              htmlFor="password"
              className="block font-[Oswald] text-[11px] tracking-[2px] uppercase text-[#969696] mb-2"
            >
              Mot de passe
            </label>
            <input
              id="password"
              name="password"
              type="password"
              autoComplete="current-password"
              className="w-full bg-transparent border-0 border-b border-[#e1e1e1] py-2 font-['Libre_Baskerville'] text-[15px] text-[#121212] outline-none focus:border-[#121212] transition-colors"
            />
            {errors.password && (
              <p className="mt-1 font-[Oswald] text-[11px] tracking-[1px] text-[#e51940]">
                {errors.password}
              </p>
            )}
          </div>

          <button
            type="submit"
            disabled={isPending}
            className="w-full py-4 bg-[#121212] text-white font-[Oswald] text-[13px] tracking-[2.5px] uppercase disabled:opacity-40 disabled:cursor-not-allowed transition-opacity"
          >
            {isPending ? 'Connexion…' : 'Se connecter'}
          </button>
        </form>
      </div>
    </div>
  )
}
