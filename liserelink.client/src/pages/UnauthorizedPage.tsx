import { useNavigate } from 'react-router-dom';

export default function UnauthorizedPage() {
  const navigate = useNavigate();

  return (
    <div className="min-h-screen bg-[#f9f4ef] flex flex-col items-center justify-center px-6">
      <h1 className="font-['Libre_Baskerville'] text-2xl text-[#121212] mb-2">
        Accès non autorisé
      </h1>
      <p className="font-[Oswald] text-[11px] tracking-[2px] uppercase text-[#969696] mb-10">
        Vous n&apos;avez pas les droits nécessaires
      </p>
      <button
        onClick={() => navigate('/login')}
        className="px-8 py-4 bg-[#121212] text-white font-[Oswald] text-[13px] tracking-[2.5px] uppercase"
      >
        Retour à la connexion
      </button>
    </div>
  );
}
