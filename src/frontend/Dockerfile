# Etapa 1 – Construcción del frontend con React
FROM node:16.9.0-alpine as builder
WORKDIR /app

# Copiar dependencias e instalarlas
COPY package*.json /app/
RUN npm install --only=production

# Copiar todo el proyecto
COPY ./ /app/

# Construir React (genera carpeta /build)
RUN npm run build

# Etapa 2 – Imagen final con Nginx
FROM nginx:1.20.1-alpine

# Copiar configuración de Nginx y archivos necesarios
COPY --from=builder /app/nginx.conf /etc/nginx/nginx.conf
COPY --from=builder /app/cloudflare-ips.conf /var/www-allow/cloudflare-ips.conf
COPY --from=builder /app/build /usr/share/nginx/html

# Copiar el script de arranque
COPY --from=builder /app/entrypoint.sh /entrypoint.sh

# 🔹 Dar permisos de ejecución al script
RUN chmod +x /entrypoint.sh

# 🔹 Usar sh para ejecutarlo (más robusto en Alpine)
ENTRYPOINT ["/bin/sh", "/entrypoint.sh"]
