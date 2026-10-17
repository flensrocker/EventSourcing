module.exports = {
  '/api/**': {
    target: process.env.PLAYGROUND_WEBAPI_HTTP,
    secure: false,
    changeOrigin: true,
  },
};
