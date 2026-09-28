/** @type {import('next').NextConfig} */
const nextConfig = {
    async rewrites() {
        return [
            {
                source: '/api/:path*',
                destination: 'http://localhost:5001/api/:path*'
            },
            {
                source: '/uploads/:path*',
                destination: 'http://localhost:5001/uploads/:path*'
            }
        ]
    }
};

const withPWA = require('@ducanh2912/next-pwa').default({
    dest: 'public',
    disable: process.env.NODE_ENV === 'development',
});

module.exports = withPWA(nextConfig);
