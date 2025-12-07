module.exports = {
  '/api/**': {
    target:
      process.env['services__playground-webapi__https__0'] ||
      process.env['services__playground-webapi__http__0'],
    secure: process.env['NODE_ENV'] !== 'development',
  },
};
