import { Module } from '@nestjs/common';
import { ConfigModule } from '@nestjs/config';
import { validateEnv } from './config/env.js';
import { LodgesModule } from './lodges/lodges.module.js';

@Module({
  imports: [
    ConfigModule.forRoot({
      envFilePath: ['.env.development.local'],
      isGlobal: true,
      validate: validateEnv,
    }),
    LodgesModule,
  ],
})
export class AppModule {}
