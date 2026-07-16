/**
 * 日志级别枚举
 */
export enum LogLevel {
  DEBUG = 'DEBUG',
  INFO = 'INFO',
  WARN = 'WARN',
  ERROR = 'ERROR'
}

/**
 * 日志接口
 */
interface LogEntry {
  level: LogLevel
  timestamp: string
  module: string
  message: string
  data?: any
}

/**
 * 日志工具类
 */
class Logger {
  private logs: LogEntry[] = []
  private maxLogs: number = 1000
  private enableConsole: boolean = true

  /**
   * 设置是否启用控制台输出
   */
  setEnableConsole(enabled: boolean): void {
    this.enableConsole = enabled
  }

  /**
   * 记录调试日志
   */
  debug(module: string, message: string, data?: any): void {
    this.log(LogLevel.DEBUG, module, message, data)
  }

  /**
   * 记录信息日志
   */
  info(module: string, message: string, data?: any): void {
    this.log(LogLevel.INFO, module, message, data)
  }

  /**
   * 记录警告日志
   */
  warn(module: string, message: string, data?: any): void {
    this.log(LogLevel.WARN, module, message, data)
  }

  /**
   * 记录错误日志
   */
  error(module: string, message: string, error?: any): void {
    this.log(LogLevel.ERROR, module, message, error)
  }

  /**
   * 内部日志记录方法
   */
  private log(level: LogLevel, module: string, message: string, data?: any): void {
    const entry: LogEntry = {
      level,
      timestamp: new Date().toISOString(),
      module,
      message,
      data
    }

    this.logs.push(entry)

    if (this.logs.length > this.maxLogs) {
      this.logs.shift()
    }

    if (this.enableConsole) {
      this.printToConsole(entry)
    }
  }

  /**
   * 打印日志到控制台
   */
  private printToConsole(entry: LogEntry): void {
    const prefix = `[${entry.timestamp}] [${entry.level}] [${entry.module}]`

    switch (entry.level) {
      case LogLevel.DEBUG:
        console.debug(prefix, entry.message, entry.data || '')
        break
      case LogLevel.INFO:
        console.info(prefix, entry.message, entry.data || '')
        break
      case LogLevel.WARN:
        console.warn(prefix, entry.message, entry.data || '')
        break
      case LogLevel.ERROR:
        console.error(prefix, entry.message, entry.data || '')
        break
    }
  }

  /**
   * 获取所有日志
   */
  getLogs(): LogEntry[] {
    return [...this.logs]
  }

  /**
   * 按级别过滤日志
   */
  getLogsByLevel(level: LogLevel): LogEntry[] {
    return this.logs.filter(log => log.level === level)
  }

  /**
   * 按模块过滤日志
   */
  getLogsByModule(module: string): LogEntry[] {
    return this.logs.filter(log => log.module === module)
  }

  /**
   * 清空日志
   */
  clearLogs(): void {
    this.logs = []
  }

  /**
   * 导出日志为JSON
   */
  exportLogs(): string {
    return JSON.stringify(this.logs, null, 2)
  }
}

export const logger = new Logger()

export default logger
