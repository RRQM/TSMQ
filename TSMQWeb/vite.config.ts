import { defineConfig } from 'vite'
import vue from '@vitejs/plugin-vue'
import vueDevTools from 'vite-plugin-vue-devtools'
import { resolve } from 'path'
import http from 'node:http'

const proxyAgent = new http.Agent({
  keepAlive: true
})

export default defineConfig({
  plugins: [vue(), vueDevTools()],
  resolve: {
    alias: {
      '@': resolve(__dirname, 'src')
    }
  },
  server: {
    port: 3000,
    proxy: {
      '/api': {
        target: 'http://127.0.0.1:7789',
        agent: proxyAgent,
        changeOrigin: true,
        rewrite: (path) => path.replace(/^\/api/, ''),
        configure: (proxy) => {
          proxy.on('error', (err, _req, res) => {
            console.log('[proxy error]', err.message)
            if ('writeHead' in res && !res.headersSent) {
              res.writeHead(500, { 'Content-Type': 'application/json' })
              res.end(JSON.stringify({ Code: 0, Message: '代理连接失败：' + err.message }))
            }
          })
        }
      }
    }
  },
  build: {
    outDir: '../TSMQServer/TSMQServer.Web/build',
    emptyOutDir: true,
    sourcemap: false
  },
  css: {
    devSourcemap: true
  }
})
