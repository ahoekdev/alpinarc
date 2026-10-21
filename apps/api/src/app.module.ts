import { Module } from '@nestjs/common';
import { ConfigModule } from '@nestjs/config';
import { LodgesModule } from './lodges/lodges.module.js';

@Module({
  imports: [
    ConfigModule.forRoot({
      envFilePath: ['.env.development.local'],
      isGlobal: true,
    }),
    LodgesModule,
  ],
})
export class AppModule {}
