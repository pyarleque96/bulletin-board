/** @type {import('tailwindcss').Config} */
module.exports = {
    // Escanea los .razor y .html de ambos proyectos para detectar todas las clases en uso.
    content: [
        './**/*.{razor,cshtml,html}',
        '../Bulletin.Board.Web.Client/**/*.{razor,cshtml,html}',
    ],
    theme: {
        extend: {
            fontFamily: {
                sans: ['DM Sans', 'system-ui', 'sans-serif'],
            },
            colors: {
                brand: {
                    50:  '#eff6ff',
                    100: '#dbeafe',
                    200: '#bfdbfe',
                    300: '#93c5fd',
                    400: '#60a5fa',
                    500: '#3B82F6',
                    600: '#2563eb',
                    700: '#1d4ed8',
                    800: '#1e40af',
                    900: '#1e3a8a',
                },
                accent: {
                    DEFAULT: '#F97316',
                    50:  '#fff7ed',
                    100: '#ffedd5',
                    400: '#fb923c',
                    500: '#f97316',
                    600: '#ea580c',
                },
                surface: '#F8FAFC',
                ink: '#1E293B',
                muted: '#64748B',
            },
            boxShadow: {
                'soft':  '0 4px 24px -4px rgba(30,41,59,0.08)',
                'lift':  '0 12px 40px -8px rgba(30,41,59,0.14)',
                'float': '0 24px 64px -12px rgba(30,41,59,0.18)',
            },
            backgroundImage: {
                'hero-gradient':  'linear-gradient(145deg, #071526 0%, #0D1E33 45%, #132840 80%, #1A3350 100%)',
                'badge-sdvip':    'linear-gradient(135deg, #fbbf24, #f59e0b)',
                'badge-vip':      'linear-gradient(135deg, #fb923c, #f97316)',
            },
            transitionTimingFunction: {
                'spring': 'cubic-bezier(0.32,0.72,0,1)',
            },
        },
    },
    plugins: [],
};
